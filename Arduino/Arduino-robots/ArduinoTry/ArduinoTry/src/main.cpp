// uncomment this for dev mode (this is a trick to save precious memory space)
// #define DEVMODE 1

#include <Arduino.h>
#include <Servo.h>
#include <NewSonar.h>
#include <RGBLed.h>
#include <LedMatrix.h>
#include <LedMatrix_constants.h>
#include <Thermosensor.h>


// NOTES
// - "println" adds "\n" at the end, so you don't need to add it manually
// - all varialbles declared in the outmost scope are "global"


// ______________________________________________________________________________________________ GLOBALS


#define EMPTY_STRING " "
#define DELIMITER ":"
#define MSG_DELIMITER "_"
#define ARDUINO_OK_FLAG "OK"
#define REQUEST_ARDUINO_RESET "RESET"
#define ARDUINO_READY_FLAG "READY"
#define RASP_DEFAULT "A"

// --- RECEIVED MESSAGES KEYS
// TRISKAR BASE
#define BASE_FORWARD_KEY "BF"
#define BASE_STRIFE_KEY "BS"
#define BASE_ANGULAR_KEY "BB"
// SERVO ARM
#define SERVO_PETALS_KEY "SP"
// LGB LED
#define RGB_LED_KEY "L"
// EYE MATRIX
#define EYE_MATRIX_CENTER_X_KEY "EX"
#define EYE_MATRIX_CENTER_Y_KEY "EY"
// TERMO SENSOR
#define TERMO_SENSOR_KEY "T"


// SETUP
#define MAX_READ_MSGS 5  // max number of messages read per iteration


// FLAGS
bool dirty;

String current_data = EMPTY_STRING;

struct KeyValueMsg {
  String key;
  String value;  
};

KeyValueMsg current_msg = {"", ""}; // global variable with the current received message

// WATCHDOG
unsigned long last_command_time;
unsigned long current_time;


// ______________________________________________________________________________________________ MOTORS
//
// LEGEND
// - MR (Motor Right)
// - ML (Motor Left)
// - MB (Motor Back)

// Maximum speed wanted. It's defined by raspberry and sent to Arduino on setup
float _MAX_SPEED = 5;  //cm/s prev: 80
float _MAX_ANGULAR = 1;  //rad/s prev 6.23

// driver pins
// NB since the NEWPING LIBRARY uses a TIMER connected to PWM PINS 9 and 10, 
//    any motor connected to those PINs will not work: avoid PINs 9 and 10
#define _MR_A 3
#define _MR_B 5
#define _ML_A 7
#define _ML_B 4
#define _MB_A 8
#define _MB_B 6

#define MAX_DC_MOTOR_SIGNAL 50 //255

// ROBOT PROPORTIONS
#define NMOTOR 3
#define wheel_radius  3.5f //cm
#define robot_radius  12.5f  //cm
#define m1_R     (-1.0f / wheel_radius)
#define mL_R     (-robot_radius / wheel_radius)
#define C60_R    (0.500000000f / wheel_radius)   // cos(60°) / R
#define C30_R    (0.866025404f / wheel_radius)   // cos(30°) / R

int counter = 0;

// ---gloabl speed targets
// TRISKAR BASE
float forward_speed;
float strafe_speed;
float angular_speed;


void set_wheel_speed(float speed, int pinA, int pinB){
  
  // Make sure the speed is within the limit.
  if (speed > MAX_DC_MOTOR_SIGNAL) {
    speed = MAX_DC_MOTOR_SIGNAL;
  } else if (speed < -MAX_DC_MOTOR_SIGNAL) {
    speed = -MAX_DC_MOTOR_SIGNAL;
  }
  
  // Set the speed and direction.
  if (speed >= 0) {
    analogWrite(pinA, speed);
    analogWrite(pinB, 0);
  } else {
    analogWrite(pinA, 0);
    analogWrite(pinB, -speed);
  }
}

void set_wheel_speeds() {

  // MULTIPLY FORWARD SPEED BY 550
  // MULTIPLY STRAFE SPEED BY 550
  // MULTIPLY angANGULAR SPEED BY 250
  
  const float dx12 = C60_R * strafe_speed * 500;
  const float dy12 = C30_R * forward_speed * 500;
  const float dthz123 = mL_R * angular_speed * 40;

  const float speed_target_R = dx12 + dy12 + dthz123; //motore anteriore dx
  const float speed_target_B = (m1_R * strafe_speed * 500) + dthz123; // motore posteriore
  const float speed_target_L = dx12 - dy12 + dthz123; //motore anteriore sx

  set_wheel_speed(speed_target_R, _MR_A, _MR_B);
  set_wheel_speed(speed_target_B, _MB_A, _MB_B);
  set_wheel_speed(speed_target_L, _ML_A, _ML_B);
}


// ______________________________________________________________________________________________ UTILS

float min_speed = 0.01;

void clamp_speeds_to_zero() {
  if (abs(strafe_speed) < min_speed)
    strafe_speed = 0;
  if (abs(forward_speed) < min_speed)
    forward_speed = 0;
  if (abs(angular_speed) < min_speed)
    angular_speed = 0;
}

void write_key_value_serial(char * key, float val) {
  Serial.print(key);
  Serial.print(':');
  Serial.println(val);
}

void write_key_value_serial(char * key, unsigned long val) {
  Serial.print(key);
  Serial.print(':');
  Serial.println(val);
}


// ______________________________________________________________________________________________ SERIAL

bool read_serial() {
  
  if (Serial.available() > 0) {
    current_data = Serial.readStringUntil('\n');
    return true;    
  }
  else
    return false;
}

void write_serial(String msg) {
    Serial.println(msg);
}

void message_response(KeyValueMsg keyValueMsg){

  // act based on a message - discriminate using the key

  String key = keyValueMsg.key;
  String value = keyValueMsg.value;

  if(key == BASE_FORWARD_KEY) {
    forward_speed = value.toFloat();
    dirty = true;
    last_command_time = millis();
    //write_serial("Received: " + key+":"+value+"\n");
    
    }
  else if (key == BASE_STRIFE_KEY) {
    strafe_speed = value.toFloat();
    dirty = true;
    last_command_time = millis();
    //write_serial("Received: " + key+":"+value+"\n");
    }
  else if (key == BASE_ANGULAR_KEY) {
    angular_speed = value.toFloat();
    dirty = true;
    last_command_time = millis();
    //write_serial("Received: " + key+":"+value+"\n");
    
    }
  else {
    write_serial("[ERROR] unsupported message key: " + key);
  }
}

void get_key_value(String msg) {
  // 1 find the position of the delimiter
  // 2 get the first and second substrings: the KEY and VALUE of the message

  int delim_index = msg.indexOf(DELIMITER);

  String key = msg.substring(0, delim_index);
  String value = msg.substring(delim_index + 1);

  // current_msg.key = key;
  // current_msg.value = value;

  KeyValueMsg tempKeyValueMsg = KeyValueMsg();
  tempKeyValueMsg.key = key;
  tempKeyValueMsg.value = value;

  message_response(tempKeyValueMsg);
}


void split_msg(String msg) {

  // split all the messages with the MSG DELIMITER
  // for each, split it into KEY-VALUE pairs and send the corresponding RESPONSE
  // at each point in time we need two indexes: the STARTING msg index, and the TERMINATING msg index
  // - the START index is either 0 or the index of the first available MSG_DELIMITER
  // - the END index is either the index of a MSG_DELIMITER or the end of the message
  // CASES
  // - only one messages: there are no delimiters
  //          * start index = 0
  //          * end index = string len
  // - more than one message: there is at least one deimiter
  //          * start index = 0 for first one; last delimiter index + 1 for all the rest
  //          * end index = string len for the last one, 

  // do nothing if it's the DEFAULT EMPTY MSG
  if (current_data == RASP_DEFAULT)
    return;

  int startDelimiterIndex = -1;
  int endDelimiterIndex = msg.indexOf(MSG_DELIMITER);

  if (endDelimiterIndex == -1) {
    // CASE 1: no delimiters, only one message
    get_key_value(msg);
    return;
  }

  bool firstRound = true;

  while (true) {

    if (firstRound) {
      startDelimiterIndex = -1;
      endDelimiterIndex = msg.indexOf(MSG_DELIMITER);
      firstRound = false;

    } else {
      startDelimiterIndex = endDelimiterIndex;
      endDelimiterIndex = msg.indexOf(MSG_DELIMITER, startDelimiterIndex + 1);
    }
    
    if (endDelimiterIndex == -1) {
      // it's the last message
      get_key_value(msg.substring(startDelimiterIndex + 1));
      break;
    }  

    get_key_value(msg.substring(startDelimiterIndex + 1, endDelimiterIndex));
  }
}


bool read_key_value_serial(){

    if (read_serial()){
      split_msg(current_data);
      return true;
    }
    else {
      return false;  
    }
}

// you can only use this with the NON-BLOCKING raspberry "send_serial" method
// otherwise you'll get rubbish
void resend_data() { 
  if (Serial.available() > 0) {
    String current_data = Serial.readStringUntil('\n');
    write_serial("you sent me: " + current_data);
  }
}

void print_msg_feedback() {
  write_serial("[MESSAGE RECEIVED] - key: " + current_msg.key + " - value: " + current_msg.value + " -> current speeds:: F:" + String(forward_speed) + " S:" + String(strafe_speed) + " A:" + String(angular_speed) + " :: - max longitudinal speed: " + String(_MAX_SPEED) + " :: - max angular speed: " + String(_MAX_ANGULAR));  
}


// ______________________________________________________________________________________________ WATCHDOG

// if after 'MAX_WATCHDOG_ELAPSED_TIME' time in milliseconds there is no input signal from serial, 
// assume central control has died, and stop everything
#define MAX_WATCHDOG_ELAPSED_TIME 7000


void reset_all_target_speeds() {
  // - set all speeds to 0
  // - set the speeds ('set_wheel_speeds' is normally only triggered when new message arrives)
  // - update also 'last_command_time' so that the watchdog_tick method does not keep triggering 
  // (but only will every 'MAX_WATCHDOG_ELAPSED_TIME' ms if nothing happens)
  
  // WHEEL BASE
  forward_speed = 0;
  strafe_speed = 0;
  angular_speed = 0;
  set_wheel_speeds();

  last_command_time = millis();
}

void watchdog_tick() {
  current_time = millis();
  
  if (current_time - last_command_time > MAX_WATCHDOG_ELAPSED_TIME){
    reset_all_target_speeds();
  }
}


// -------------------------------------------------------------------------------------- MAIN

int prevcTime = 0;
int maxtimee = 2000; 
bool canWrite = false;
unsigned int lastWriteTime = 0;

void initialize() {

  // watchdog times
  last_command_time = millis();
  current_time = millis();

  // motor speed targets
  forward_speed = 0;
  strafe_speed = 0;
  angular_speed = 0;
  

  // initialization flags
  dirty = false;
}

void final_setup() {
  write_serial("[SETUP] internal setup complete");
}

void setup() {

  // 1 start serial and await a bit for warmup;
  Serial.begin(500000);
  delay(200);

  // 2 initialize variables
  initialize();

  
  // wrapping up
  final_setup();
  canWrite = false;
  lastWriteTime = millis();

  write_serial(ARDUINO_OK_FLAG);
  
  Serial.setTimeout(5000);

  prevcTime = millis();

  // TEST

  delay(50);

}  
  
// how long we wait for a message to arrive before we write anyway
unsigned int maxWriteElapsed = 5000;
unsigned int now;
unsigned int diff;

unsigned int serial_elapsed;
unsigned int min_serial_elapsed = 5;
unsigned int last_serial_time = millis();

bool isMsg = false;


void serial_loop() {

  // a serial loop can only be performed once every SERIAL_ELAPSED time
  now = millis();

  serial_elapsed = now - last_serial_time;
  if (serial_elapsed < min_serial_elapsed)
    return;

  // read everything it can
  while (read_key_value_serial()){
    isMsg = true;
  }

  // if at least a msg was received, use the updated values
  if (isMsg) {
    clamp_speeds_to_zero();

    set_wheel_speeds();


    canWrite = true;

    last_serial_time = millis();

    isMsg = false;
  }
}


void loop() {  

  watchdog_tick();

  // the EYE needs a dedicated step for the BLINK, to be executed at every timestep
  

  serial_loop();
} 
