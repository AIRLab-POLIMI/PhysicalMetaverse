// uncomment this for dev mode (this is a trick to save precious memory space)
// #define DEVMODE 1

#include <Arduino.h>
#include <Servo.h>
#include "TriskarBase.h"



//-----------------------Globals

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

//_______________________________________________________________TRISKAR_BASE
// LEGEND
// - MR (Motor Right)
// - ML (Motor Left)
// - MB (Motor Back)

// Maximum speed wanted. It's defined by raspberry and sent to Arduino on setup
float _MAX_SPEED = 80;  //cm/s
float _MAX_ANGULAR = 6.23;  //rad/s

#define _MR_A 3
#define _MR_B 5
#define _ML_A 7
#define _ML_B 4
#define _MB_A 8
#define _MB_B 6
TriskarBaseMid triskarBase;  // BIG TRISKAR BASE (Sonoma) 
void write_serial(String msg) {
    Serial.println(msg);
}

void message_response(KeyValueMsg keyValueMsg){
  // act based on a message - discriminate using the key

  String key = keyValueMsg.key;
  String value = keyValueMsg.value;

  if(key == BASE_FORWARD_KEY) {
    triskarBase.setForwardTarget(value.toFloat());
    Serial.println("FORWARD : " + String(value.toFloat()));
    dirty = true;
    last_command_time = millis();
    }
  else if (key == BASE_STRIFE_KEY) {
    triskarBase.setStrafeTarget(value.toFloat());
    Serial.println("STRAFE : " + String(value.toFloat()));
    dirty = true;
    last_command_time = millis();
    }
  else if (key == BASE_ANGULAR_KEY) {
    triskarBase.setAngularTarget(value.toFloat());
    Serial.println("ANGULAR : " + String(value.toFloat()));
    dirty = true;
    last_command_time = millis();
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

//Split Message read from serial
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

//Read data from serial
bool read_serial() {
  
  if (Serial.available() > 0) {
    current_data = Serial.readStringUntil('\n');
    write_serial("AVAILABLE");
    return true;    
  }
  else
  {
    return false;
  }
}

//Returns if a message is arrived on serial
bool read_key_value_serial(){

    if (read_serial()){
      split_msg(current_data);
      write_serial("READ MSG FROM SERIAL");
      return true;
    }
    else {
      return false;  
    }
}

// if after 'MAX_WATCHDOG_ELAPSED_TIME' time in milliseconds there is no input signal from serial, 
// assume central control has died, and stop everything
#define MAX_WATCHDOG_ELAPSED_TIME 100000


void reset_all_target_speeds() {
  // - set all speeds to 0
  // - set the speeds ('set_wheel_speeds' is normally only triggered when new message arrives)
  // - update also 'last_command_time' so that the watchdog_tick method does not keep triggering 
  // (but only will every 'MAX_WATCHDOG_ELAPSED_TIME' ms if nothing happens)
  
  // WHEEL BASE
  triskarBase.setSpeedTargets(0, 0, 0);
  triskarBase.setWheelSpeeds();

  last_command_time = millis();
}

void watchdog_tick() {
  current_time = millis();
  
  if (current_time - last_command_time > MAX_WATCHDOG_ELAPSED_TIME){
    reset_all_target_speeds();
  }
}

int prevcTime = 0;
int maxtimee = 2000;
bool canWrite = false;
unsigned int lastWriteTime = 0;

// how long we wait for a message to arrive before we write anyway
unsigned int maxWriteElapsed = 5000;
unsigned int now;
unsigned int diff;

unsigned int serial_elapsed;
unsigned int min_serial_elapsed = 5;
unsigned int last_serial_time = millis();

bool isMsg = false;

//Set values to servos
void serial_loop() {
  // a serial loop can only be performed once every SERIAL_ELAPSED time
  now = millis();

  serial_elapsed = now - last_serial_time;
  if (serial_elapsed < min_serial_elapsed)
    return;

  // read everything it can from serial
  while (read_key_value_serial() && !isMsg){
    isMsg = true;
  }

  // if at least a msg was received, use the updated values
  if (isMsg) {
    write_serial("IS MSG");
    triskarBase.setWheelSpeeds();

    canWrite = true;

    last_serial_time = millis();

    isMsg = false;
  }
}

void setup() {
  // 1 start serial and await a bit for warmup;
  Serial.begin(115200);
  delay(200);

  // 3.1 setup wheel base
  triskarBase.setup(_MR_A, _MR_B, _ML_A, _ML_B, _MB_A, _MB_B);

  canWrite = false;
  lastWriteTime = millis();

  write_serial(ARDUINO_OK_FLAG);
  
  Serial.setTimeout(5000);

  prevcTime = millis();

}

//Set values to triskarBase motors
void speeds_loop() {

  int a = 1;

  triskarBase.setSpeedTargets(a, 0, 0);
  Serial.println("FORWARD 1");
  triskarBase.setWheelSpeeds();
  delay(2000);

  triskarBase.setSpeedTargets(0, a, 0);
  Serial.println("STRAFE 1");
  triskarBase.setWheelSpeeds();
  delay(2000);

  triskarBase.setSpeedTargets(0, 0, a);
  Serial.println("ANGULAR 1");
  triskarBase.setWheelSpeeds();
  delay(2000);

  triskarBase.setSpeedTargets(-a, 0, 0);
  Serial.println("FORWARD -1");
  triskarBase.setWheelSpeeds();
  delay(2000);

  triskarBase.setSpeedTargets(0, -a, 0);
  Serial.println("STRAFE -1");
  triskarBase.setWheelSpeeds();
  delay(2000);

  triskarBase.setSpeedTargets(0, 0, -a);
  Serial.println("ANGULAR -1");
  triskarBase.setWheelSpeeds();
  delay(2000);
}

void loop() {
  //watchdog_tick();
  serial_loop();
  //Set values to triskarBase
  //speeds_loop();
}