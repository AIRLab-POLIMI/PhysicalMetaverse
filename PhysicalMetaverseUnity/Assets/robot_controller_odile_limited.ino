#include <Adafruit_MPR121.h>

#include <Adafruit_PWMServoDriver.h>
#include <VarSpeedServo.h>
#include <Wire.h>

String n;
//-----------------------Globals

#define EMPTY_STRING " "
#define DELIMITER ":"
#define MSG_DELIMITER "_"
#define ARDUINO_OK_FLAG "OK"
#define REQUEST_ARDUINO_RESET "RESET"
#define ARDUINO_READY_FLAG "READY"
#define RASP_DEFAULT "A"

// --- RECEIVED MESSAGES KEYS
//SERVOS
#define TAIL_BF "TBF"
#define TAIL_UD "TUD"
#define TAIL_LR "TLR"
#define HEAD_BF "HBF"
#define HEAD_UD "HUD"
#define HEAD_LR "HLR"
//MOODS
#define MOODS_KEY "MT" 

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

//Capacitive sensor
Adafruit_MPR121 cap = Adafruit_MPR121();
bool detected = false;
byte touchStatus;
bool released = 0;

//_______________________________________________________________SERVOS
//HEAD PINS
#define PIN_SERVO_HEAD_BEAK_T2 2
#define PIN_SERVO_HEAD_BEAK_P 3
#define PIN_SERVO_HEAD_BEAK_T1 4
#define PIN_SERVO_HEAD_BODY_T 22

//TAIL PINS
#define PIN_SERVO_TAIL_BEAK_T2 5
#define PIN_SERVO_TAIL_BEAK_P 6
#define PIN_SERVO_TAIL_BEAK_T1 7
#define PIN_SERVO_TAIL_NECK_T 8
#define PIN_SERVO_TAIL_BODY_T 24
#define PIN_SERVO_TAIL_BODY_P 9

//HEAD ANGLES
#define MIN_ANGLE_HEAD_BEAK_T2 20
#define MAX_ANGLE_HEAD_BEAK_T2 80
#define MIN_ANGLE_HEAD_BEAK_P 1
#define MAX_ANGLE_HEAD_BEAK_P 179
#define MIN_ANGLE_HEAD_BEAK_T1 40
#define MAX_ANGLE_HEAD_BEAK_T1 140
#define MIN_ANGLE_HEAD_BODY_T 75
#define MAX_ANGLE_HEAD_BODY_T 140

//TAIL ANGLES
#define MIN_ANGLE_TAIL_BEAK_T2 70
#define MAX_ANGLE_TAIL_BEAK_T2 160
#define MIN_ANGLE_TAIL_BEAK_P 5
#define MAX_ANGLE_TAIL_BEAK_P 175
#define MIN_ANGLE_TAIL_BEAK_T1 85
#define MAX_ANGLE_TAIL_BEAK_T1 165
#define MIN_ANGLE_TAIL_NECK_T 60
#define MAX_ANGLE_TAIL_NECK_T 90
#define MIN_ANGLE_TAIL_BODY_T 80
#define MAX_ANGLE_TAIL_BODY_T 130
#define MIN_ANGLE_TAIL_BODY_P 110
#define MAX_ANGLE_TAIL_BODY_P 130

#define MIN_SIN 0
#define MAX_SIN 255

#define MIN_FREQ_HYS 0
#define MAX_FREQ_HYS 5000
#define MIN_AMPL_HYS 0
#define MAX_AMPL_HYS 5
#define SERVO_SPEED 20

#define SERVOMIN  200 // this is the 'minimum' pulse length count (out of 4096)
#define SERVOMAX  575 // this is the 'maximum' pulse length count (out of 4096)

//#define SERVO_m (float)(PETAL_ANGLE_OPENED - PETAL_ANGLE_CLOSED)

//Servo motors
VarSpeedServo servo_head_beak_t2;
VarSpeedServo servo_head_beak_p;
VarSpeedServo servo_head_beak_t1;
VarSpeedServo servo_head_body;
VarSpeedServo servo_tail_beak_t2;
VarSpeedServo servo_tail_beak_p;
VarSpeedServo servo_tail_beak_t1;
VarSpeedServo servo_tail_neck;
VarSpeedServo servo_tail_body;
VarSpeedServo servo_tail_body_p;

float T_BF_pos; //tail tilt b
float T_T1_pos; //tail tilt 1
float T_UD_pos; //tail tilt 2
float T_TN_pos; //tail tilt n
float T_LR_pos; //tail pan b
float T_P1_pos; //tail pan h
float B_BF_pos; //beak tilt 2
float B_TB_pos; //beak tilt b
float B_UD_pos; //beak tilt 1
float B_LR_pos; //beak pan

float T_tilt2_add;
float T_tilt1_add;
float T_pan_add;
float B_tilt2_add;

int T_BF_angle; //tail tilt b
int T_T1_angle; //tail tilt 1
int T_UD_angle; //tail tilt 2
int T_TN_angle; //tail tilt n
int T_LR_angle; //tail pan b
int T_P1_angle; //tail pan h
int B_BF_angle; //beak tilt 2
int B_TB_angle; //beak tilt b
int B_UD_angle; //beak tilt 1
int B_LR_angle; //beak pan

int mood_value;
int freq_value;
int ampl_value;
int val_amp_hys = 0;
int val_freq_hys = 0;

//______HYSTERIA VAR
int start_duration = millis();
int curr_time;
bool ON = true;
//__________________

//______SIN VAR
bool running = false;

int servo_zero = 1420;

const int quadrant_steps_min = 8; //highest speed 6*4*20ms = 480ms -> 2.08Hz
const int quadrant_steps_max = 40;  //lowest speed 60*4*20ms = 4800ms -> 0.208Hz
const int servo_amplitude_step = 10;     // pwm pulse width +/- [us] change per quadrant
const int servo_amplitude_min = 20;     // pwm pulse width +/- [us] for max upper/lower position
int quadrant_steps = quadrant_steps_max;

int servo_amplitude_max = 100; //200
int servo_amplitude = servo_amplitude_min;
const int step_duration = 20;
int servo_val_us;

volatile int speed_position;
volatile int amplitude_position;

int enter_loop;
int end_loop;

int scale2us(int angle){
  return angle*50/9; 
} 

Adafruit_PWMServoDriver pwm = Adafruit_PWMServoDriver();
int servos[10] = {MIN_ANGLE_HEAD_BEAK_T2, MIN_ANGLE_HEAD_BEAK_P, MIN_ANGLE_HEAD_BEAK_T1, MIN_ANGLE_TAIL_BEAK_T2, MIN_ANGLE_TAIL_BEAK_P, MIN_ANGLE_TAIL_BEAK_T1, MIN_ANGLE_TAIL_NECK_T, scale2us(MIN_ANGLE_TAIL_BODY_P), 90, 90};
int upperLimits[10] = {MAX_ANGLE_HEAD_BEAK_T2, MAX_ANGLE_HEAD_BEAK_P, MAX_ANGLE_HEAD_BEAK_T1, MAX_ANGLE_TAIL_BEAK_T2, MAX_ANGLE_TAIL_BEAK_P, MAX_ANGLE_TAIL_BEAK_T1, MAX_ANGLE_TAIL_NECK_T, MAX_ANGLE_TAIL_BODY_P, MAX_ANGLE_HEAD_BODY_T, MAX_ANGLE_TAIL_BODY_T};
int lowerLimits[10] = {MIN_ANGLE_HEAD_BEAK_T2, MIN_ANGLE_HEAD_BEAK_P, MIN_ANGLE_HEAD_BEAK_T1, MIN_ANGLE_TAIL_BEAK_T2, MIN_ANGLE_TAIL_BEAK_P, MIN_ANGLE_TAIL_BEAK_T1, MIN_ANGLE_TAIL_NECK_T, MIN_ANGLE_TAIL_BODY_P,  MIN_ANGLE_HEAD_BODY_T, MIN_ANGLE_TAIL_BODY_T};
int targetPoses[10] = {MIN_ANGLE_HEAD_BEAK_T2, MIN_ANGLE_HEAD_BEAK_P, MIN_ANGLE_HEAD_BEAK_T1, MIN_ANGLE_TAIL_BEAK_T2, MIN_ANGLE_TAIL_BEAK_P, MIN_ANGLE_TAIL_BEAK_T1, MIN_ANGLE_TAIL_NECK_T, MIN_ANGLE_TAIL_BODY_P, 90, 90};
//_______________

void write_serial(String msg) {
    Serial.println(msg);
}

int angleToPulse(int ang){
   int pulse = map(ang, 0, 180, SERVOMIN, SERVOMAX);// map angle of 0 to 180 to Servo min and Servo max
   return pulse;
}

int senseTouch(){
  touchStatus = cap.touched();
  byte sensorIdx = 3;
  released = 0;

  
  if(touchStatus & (1 << sensorIdx)) {
    if(!detected){
      Serial.println("touched");
      detected = true;    
    }
    else {
      Serial.println(".");
    }
  }
  else {
    if(detected){
      Serial.println("released");
      detected = false;
      released = 1;
    }    
  }
  return released;
}



//void hysteria_loop(float pos, VarSpeedServo servo, int ampl, int duration, int flag_us){
void hysteria_loop(){

  val_amp_hys = rand()%MAX_AMPL_HYS;
  val_freq_hys = rand()%MAX_FREQ_HYS;

  curr_time = millis() - start_duration;
  if(curr_time > val_freq_hys)
  {
    if(ON)
    {
      ON = false;

      servo_head_beak_t2.write(B_UD_angle, SERVO_SPEED, false);
      servo_head_body.write(B_BF_angle, SERVO_SPEED, true);
      servo_head_beak_p.write(B_LR_angle, SERVO_SPEED, true);
      servo_head_beak_t1.write(B_TB_angle, SERVO_SPEED*1.5, true);

      servo_tail_beak_t2.write(T_UD_angle, SERVO_SPEED, false);
      servo_tail_neck.write(T_TN_angle, SERVO_SPEED, true);
      servo_tail_body.write(T_BF_angle, SERVO_SPEED, false);
      servo_tail_beak_t1.write(T_T1_angle, SERVO_SPEED*2.7, true);
      servo_tail_beak_p.write(T_P1_angle, SERVO_SPEED, false);
      servo_tail_body_p.writeMicroseconds(scale2us(T_LR_angle));
    }
    else
    {
      ON = true;
      /*servo_head_beak_t2.write(constrain(head_beak_t2_angle + pow(-1,(rand()%2+1))*val_amp_hys, MIN_ANGLE_HEAD_BEAK_T2, MAX_ANGLE_HEAD_BEAK_T2));
      servo_head_beak_p.write(constrain(head_beak_p_angle + pow(-1,(rand()%2+1))*val_amp_hys, MIN_ANGLE_HEAD_BEAK_P, MAX_ANGLE_HEAD_BEAK_P));
      servo_head_beak_t1.write(constrain(head_beak_t1_angle + pow(-1,(rand()%2+1))*val_amp_hys, MIN_ANGLE_HEAD_BEAK_T1, MAX_ANGLE_HEAD_BEAK_T1));
      servo_head_body.write(constrain(head_body_t_angle + pow(-1,(rand()%2+1))*val_amp_hys, MIN_ANGLE_HEAD_BODY_T, MAX_ANGLE_HEAD_BODY_T));
      */
      servo_head_beak_t2.write(B_UD_angle, SERVO_SPEED, false);
      servo_head_body.write(B_BF_angle, SERVO_SPEED, true);
      servo_head_beak_p.write(B_LR_angle, SERVO_SPEED, true);
      servo_head_beak_t1.write(B_TB_angle, SERVO_SPEED*1.5, true);
      servo_tail_beak_t2.write(constrain(T_UD_angle + pow(-1,(rand()%2+1))*val_amp_hys, MIN_ANGLE_TAIL_BEAK_T2, MAX_ANGLE_TAIL_BEAK_T2), SERVO_SPEED, false);
      servo_tail_neck.write(constrain(T_TN_angle + pow(-1,(rand()%2+1))*val_amp_hys, MIN_ANGLE_TAIL_NECK_T, MAX_ANGLE_TAIL_NECK_T), SERVO_SPEED, true);
      servo_tail_body.write(constrain(T_BF_angle + pow(-1,(rand()%2+1))*val_amp_hys, MIN_ANGLE_TAIL_BODY_T, MAX_ANGLE_TAIL_BODY_T), SERVO_SPEED, false);
      servo_tail_beak_t1.write(constrain(T_T1_angle + pow(-1,(rand()%2+1))*val_amp_hys, MIN_ANGLE_TAIL_BEAK_T1, MAX_ANGLE_TAIL_BEAK_T1), SERVO_SPEED*2.7, true);
      servo_tail_beak_p.write(constrain(T_P1_angle + pow(-1,(rand()%2+1))*val_amp_hys, MIN_ANGLE_TAIL_BEAK_P, MAX_ANGLE_TAIL_BEAK_P), SERVO_SPEED, false);
      servo_tail_body_p.writeMicroseconds(scale2us(constrain(T_LR_angle + pow(-1,(rand()%2+1))*val_amp_hys, MIN_ANGLE_TAIL_BODY_P, MAX_ANGLE_TAIL_BODY_P)));
    }
    start_duration = millis();
  }
}

//_______________________SINUSOIDAL MOVEMENT
//void sin_mov(float pos, VarSpeedServo servo, int flag_us){
void sin_mov(){
  static int quadrant = 0;
  static int step = 0;
  static int speed_position_read;
  static int amplitude_position_read;
  static unsigned long next_step = millis();

  unsigned long current_time; //milliseconds (ms)
  //speed_position = freq_value;
  //amplitude_position = ampl_value;
  speed_position = 0;
  amplitude_position = 50;

  if((quadrant == 0 || quadrant == 2) && step == 0) //speed_position != speed_position_read 
  {
    quadrant_steps = quadrant_steps - (speed_position - speed_position_read);
    //speed_position_read = speed_position;

    if(quadrant_steps < quadrant_steps_min) quadrant_steps = quadrant_steps_min;
    if(quadrant_steps > quadrant_steps_max) quadrant_steps = quadrant_steps_max;

  }
  if((quadrant == 1 || quadrant == 3) && step== 0) //amplitude_position != amplitude_position_read
  {
    if(amplitude_position > amplitude_position_read)
      servo_amplitude = servo_amplitude + servo_amplitude_step;
    else
      servo_amplitude = servo_amplitude - servo_amplitude_step;

    //amplitude_position_read = amplitude_position;

    if(servo_amplitude < servo_amplitude_min) servo_amplitude = servo_amplitude_min;
    if(servo_amplitude > servo_amplitude_max) servo_amplitude = servo_amplitude_max;    
  }

  current_time = millis();

  if(current_time >= next_step) //&& step>=quadrant_steps -- aggiungere per fare solo un movimento
  {
    next_step = current_time + step_duration;

    //servo_head_beak_t2.writeMicroseconds(constrain((1000 + (head_beak_t2_angle*150+13)/27) + servo_amplitude * sin(2*3.14/quadrant_steps*(step+quadrant*quadrant_steps)), 1000+(MIN_ANGLE_HEAD_BEAK_T2*150+13)/27, 1000+(MAX_ANGLE_HEAD_BEAK_T2*150+13)/27));
    //servo_head_beak_p.writeMicroseconds(constrain((1000 + (head_beak_p_angle*150+13)/27) + servo_amplitude * sin(2*3.14/quadrant_steps*(step+quadrant*quadrant_steps)), 1000+(MIN_ANGLE_HEAD_BEAK_P*150+13)/27, 1000+(MAX_ANGLE_HEAD_BEAK_P*150+13)/27));
    //servo_head_beak_t1.writeMicroseconds(constrain((1000 + (head_beak_t1_angle*150+13)/27) + servo_amplitude * sin(2*3.14/quadrant_steps*(step+quadrant*quadrant_steps)), 1000+(MIN_ANGLE_HEAD_BEAK_T1*150+13)/27, 1000+(MAX_ANGLE_HEAD_BEAK_T1*150+13)/27));
    //servo_head_body.writeMicroseconds(constrain((1000 + (head_body_t_angle*150+13)/27) + servo_amplitude * sin(2*3.14/quadrant_steps*(step+quadrant*quadrant_steps)), 1000+(MIN_ANGLE_HEAD_BODY_T*150+13)/27, 1000+(MAX_ANGLE_HEAD_BODY_T*150+13)/27));
    servo_head_beak_t2.write(B_UD_angle, SERVO_SPEED, false);
    servo_head_body.write(B_BF_angle, SERVO_SPEED, true);
    servo_head_beak_p.write(B_LR_angle, SERVO_SPEED, true);
    servo_head_beak_t1.write(B_TB_angle, SERVO_SPEED*1.5, true);
    servo_tail_beak_t2.writeMicroseconds(constrain((1000 + (T_UD_angle*150+13)/27) + servo_amplitude * sin(2*3.14/quadrant_steps*(step+quadrant*quadrant_steps)), 1000+(MIN_ANGLE_TAIL_BEAK_T2*150+13)/27, 1000+(MAX_ANGLE_TAIL_BEAK_T2*150+13)/27));
    servo_tail_neck.writeMicroseconds(constrain((1000 + (T_TN_angle*150+13)/27) + servo_amplitude * sin(2*3.14/quadrant_steps*(step+quadrant*quadrant_steps)), 1000+(MIN_ANGLE_TAIL_NECK_T*150+13)/27, 1000+(MAX_ANGLE_TAIL_NECK_T*150+13)/27));
    servo_tail_body.writeMicroseconds(constrain((1000 + (T_BF_angle*150+13)/27) + servo_amplitude * sin(2*3.14/quadrant_steps*(step+quadrant*quadrant_steps)), 1000+(MIN_ANGLE_TAIL_BODY_T*150+13)/27, 1000+(MAX_ANGLE_TAIL_BODY_T*150+13)/27));
    servo_tail_beak_t1.writeMicroseconds(constrain((1000 + (T_T1_angle*150+13)/27) + servo_amplitude * sin(2*3.14/quadrant_steps*(step+quadrant*quadrant_steps)), 1000+(MIN_ANGLE_TAIL_BEAK_T1*150+13)/27, 1000+(MAX_ANGLE_TAIL_BEAK_T1*150+13)/27));
    servo_tail_beak_p.writeMicroseconds(constrain((1000 + (T_P1_angle*150+13)/27) + servo_amplitude * sin(2*3.14/quadrant_steps*(step+quadrant*quadrant_steps)), 1000+(MIN_ANGLE_TAIL_BEAK_P*150+13)/27, 1000+(MAX_ANGLE_TAIL_BEAK_P*150+13)/27));
    servo_tail_body_p.writeMicroseconds(constrain(scale2us(T_LR_angle) + servo_amplitude * sin(2*3.14/quadrant_steps*(step+quadrant*quadrant_steps)), scale2us(MIN_ANGLE_TAIL_BODY_P), scale2us(MAX_ANGLE_TAIL_BODY_T)));

    step = step + 1;
    if(step>=quadrant_steps)
    {
      step = 0;
      quadrant = quadrant + 1;
      if(quadrant >= 4)
        quadrant = 0;
    }
  }
}
//___________________________________________________________-

void check_mood(float pos, VarSpeedServo servo){

  //if(mood_value == 0){
    //hysteria(pos, servo, flag_us);
  //}
  //else 
  if(mood_value == 1)
  {
    servo.write(pos);
  }
  /*else
    sin_mov();
    */
}

int set_servo_pos(float pos, VarSpeedServo servo, int MIN, int MAX) {

  // pos arrives in range [-1, 1] - rescale it in [PETAL_ANGLE_OPENED, PETAL_ANGLE_CLOSED]
  float servo_m = (float) MAX - MIN;
  pos = ((((pos+1.0f) * servo_m) / 2.0f)+(float)MIN);

  // Make sure the position is within the limit.
  // it could not be the case if the incoming normalized setpoint was not in [-1, 1]
  if (pos < MIN) {
    pos = MIN;
  } else if (pos > MAX) {
    pos = MAX;
  }

  //write_serial("pos "+String(pos) + " mean "+ String(servo_m));
  return pos;
  // write_serial("[SET SERVO POS] - target pos: " + String(servo_target_pos) + " - SPEED: " + String(speed));
}

int set_angle_perc(int pos, VarSpeedServo servo, int min_base, int max_base, int min_servo, int max_servo, bool BF){
  //write_serial("pos " + String(pos));
  float pos_servo = pos - min_base;
  //write_serial("pos servo " + String(pos_servo));
  pos_servo = 100 * pos_servo / (max_base - min_base);
  pos_servo = ((max_servo - min_servo) * pos_servo) / 100;
  if(BF)
  {  
    pos_servo = max_servo - pos_servo;
    //write_serial("pos servo " + String(pos_servo));
  }
  else
    pos_servo = min_base - pos_servo;

  //write_serial("pos "+String(pos_servo));

  return pos_servo; 
}

int set_servo_body_p_pos(float pos, VarSpeedServo servo, int MIN, int MAX) {

  // pos arrives in range [-1, 1] - rescale it in [PETAL_ANGLE_OPENED, PETAL_ANGLE_CLOSED]
  float servo_m = (float) MAX - MIN;
  //pos = (((pos + 1.0f) / 2.0f) * servo_m)  + (float)MAX;
  pos = (((pos+1.0f) * servo_m) / 2.0f) + (float)MIN;
  //pos = map(pos, -1.0, 1.0, MIN, MAX);

  // Make sure the position is within the limit.
  // it could not be the case if the incoming normalized setpoint was not in [-1, 1]
  if (pos < MIN) {
    pos = MIN;
  } else if (pos > MAX) {
    pos = MAX;
  }

  //write_serial("pos "+String(pos) + " mean "+ String(servo_m));

  return pos;
}

void set_servos_pos() {

  write_serial("set servos pos");

  B_BF_angle = set_servo_pos(B_BF_pos, servo_head_body, MIN_ANGLE_HEAD_BODY_T, MAX_ANGLE_HEAD_BODY_T); 
  B_TB_angle = set_angle_perc(B_BF_angle, servo_head_beak_t1, MIN_ANGLE_HEAD_BODY_T, MAX_ANGLE_HEAD_BODY_T, MIN_ANGLE_HEAD_BEAK_T1, MAX_ANGLE_HEAD_BEAK_T1, true);
  B_LR_angle = set_servo_pos(B_LR_pos, servo_head_beak_p, MIN_ANGLE_HEAD_BEAK_P, MAX_ANGLE_HEAD_BEAK_P);
  B_UD_angle = set_servo_pos(B_UD_pos, servo_head_beak_t2, MIN_ANGLE_HEAD_BEAK_T2, MAX_ANGLE_HEAD_BEAK_T2);
  T_UD_angle = set_servo_pos(T_UD_pos, servo_tail_beak_t2, MIN_ANGLE_TAIL_BEAK_T2, MAX_ANGLE_TAIL_BEAK_T2);
  T_TN_angle = T_UD_angle+10;
  T_LR_angle = set_servo_body_p_pos(T_LR_pos, servo_tail_body_p, MIN_ANGLE_TAIL_BODY_P, MAX_ANGLE_TAIL_BODY_P);
  write_serial("[ARDUINO] TLR angle: " + String(T_LR_angle));
  T_P1_angle = set_angle_perc(T_LR_angle, servo_tail_beak_p, MIN_ANGLE_TAIL_BODY_P, MAX_ANGLE_TAIL_BODY_P, MIN_ANGLE_TAIL_BEAK_P, MAX_ANGLE_TAIL_BEAK_P, false);
  T_BF_angle = set_servo_pos(T_BF_pos, servo_tail_body, MIN_ANGLE_TAIL_BODY_T, MAX_ANGLE_TAIL_BODY_T);
  T_T1_angle = set_angle_perc(T_BF_angle, servo_tail_beak_t1, MIN_ANGLE_TAIL_BODY_T, MAX_ANGLE_TAIL_BODY_T, MIN_ANGLE_TAIL_BEAK_T1, MAX_ANGLE_TAIL_BEAK_T1, true);

  /*if(mood_value == 1)
  {
    servo_head_beak_t2.write(B_UD_angle, SERVO_SPEED, false);
    servo_head_body.write(B_BF_angle, SERVO_SPEED, true);
    servo_head_beak_p.write(B_LR_angle, SERVO_SPEED, true);
    servo_head_beak_t1.write(B_TB_angle, SERVO_SPEED*1.5, true);
    servo_tail_beak_t2.write(T_UD_angle, SERVO_SPEED, false);
    servo_tail_neck.write(T_TN_angle, SERVO_SPEED, true);
    servo_tail_body.write(T_BF_angle, 10, false);
    servo_tail_beak_t1.write(T_T1_angle, SERVO_SPEED*2.7, true);
    servo_tail_beak_p.write(T_P1_angle, SERVO_SPEED, false);
    servo_tail_body_p.writeMicroseconds(scale2us(T_LR_angle));
  }*/

  servos[0] = B_UD_angle;
  servos[1] = B_LR_angle;
  servos[2] = B_TB_angle;
  write_serial("--------- ANGLE TB: " + String(B_TB_angle));
  servos[3] = T_UD_angle;
  servos[4] = T_P1_angle;
  servos[5] = T_T1_angle;
  servos[6] = T_TN_angle;
  //servos[7] = scale2us(T_LR_angle);
  servos[8] = B_BF_angle;
  write_serial("--------- ANGLE BF : " + String(B_BF_angle));
  servos[9] = T_BF_angle;

  write_serial("-------- ANGLE : " + String(angleToPulse(servos[2])));
  write_serial("-------- ANGLE BF : " + String(angleToPulse(servos[8])));
  
  servo_tail_body_p.writeMicroseconds(scale2us(T_LR_angle));
  //pwm.setPWM(0, 0, angleToPulse(servos[0]));
  for(int i = 0; i < 10; i++){
    pwm.setPWM(i, 0, angleToPulse(servos[i]));
    delay(5);
  }
}

void reset_all_servos() {

  // by default petals are closed
  B_UD_pos = MIN_ANGLE_HEAD_BEAK_T2;
  B_LR_pos = MIN_ANGLE_HEAD_BEAK_P;
  B_TB_pos = MIN_ANGLE_HEAD_BEAK_T1;
  B_BF_pos = 90;

  T_UD_pos = MIN_ANGLE_TAIL_BEAK_T2;
  T_P1_pos = MIN_ANGLE_TAIL_BEAK_P;
  T_T1_pos = MIN_ANGLE_TAIL_BEAK_T1;
  T_TN_pos = MIN_ANGLE_TAIL_NECK_T;
  T_BF_pos = 90;
  T_LR_pos = MIN_ANGLE_TAIL_BODY_P;

  /*servo_head_beak_t2.write(B_UD_pos);
  servo_head_beak_p.write(B_LR_pos);
  servo_head_beak_t1.write(B_TB_pos);
  servo_head_body.write(B_BF_pos);
  servo_tail_beak_t2.write(T_UD_pos);
  servo_tail_beak_p.write(T_P1_pos);
  servo_tail_beak_t1.write(T_T1_pos);
  servo_tail_neck.write(T_TN_pos);
  servo_tail_body.write(T_BF_pos);
  servo_tail_body_p.writeMicroseconds(scale2us(T_LR_pos));*/
  
  servo_tail_body_p.writeMicroseconds(scale2us(T_LR_pos));
  for(int i=0; i<10; i++){
    pwm.setPWM(i, 0, angleToPulse(servos[i]));
    targetPoses[i] = servos[i];
  }
}

void initialize_servos(){
  // attach pins
  servo_head_beak_t2.attach(PIN_SERVO_HEAD_BEAK_T2);
  servo_head_beak_p.attach(PIN_SERVO_HEAD_BEAK_P);
  servo_head_beak_t1.attach(PIN_SERVO_HEAD_BEAK_T1);
  servo_head_body.attach(PIN_SERVO_HEAD_BODY_T);
  servo_tail_beak_t2.attach(PIN_SERVO_TAIL_BEAK_T2);
  servo_tail_beak_p.attach(PIN_SERVO_TAIL_BEAK_P);
  servo_tail_beak_t1.attach(PIN_SERVO_TAIL_BEAK_T1);
  servo_tail_neck.attach(PIN_SERVO_TAIL_NECK_T);
  servo_tail_body.attach(PIN_SERVO_TAIL_BODY_T);
  servo_tail_body_p.attach(PIN_SERVO_TAIL_BODY_P);

  // brief delay
  delay(200);

  // reset all SERVO positions and AWAIT for them to be reset
  reset_all_servos();
}

float onServosValueRcv(float newVal) {
  // pos arrives in range [-1, 1]. Saturate it to those bounds.
  float var = newVal;
  if (var > 1)
    var = 1;
  if (var < -1)
    var = -1;
  return var;
}

void onMoodsValueRcv(float newVal){
  float m_temp = newVal;
  if(m_temp < -0.5) 
  {
    //HYSTERIA
    mood_value = 0;
  }
  else if(m_temp > 0.5)
  {
    //SIN
    mood_value = 2;
  }
  else
    mood_value = 1;
}

void message_response(KeyValueMsg keyValueMsg){

  // act based on a message - discriminate using the key

  String key = keyValueMsg.key;
  String value = keyValueMsg.value;
  write_serial("[ARDUINO] KEY : "+ key + " VALUE : "+ value);

  if(key == TAIL_BF) {
    T_BF_pos = onServosValueRcv(value.toFloat());
    dirty = true;
    last_command_time = millis();
    }
  else if (key == TAIL_UD) { 
    T_UD_pos = onServosValueRcv(value.toFloat());
    dirty = true;
    last_command_time = millis();
    }
  else if (key == TAIL_LR) { 
    write_serial("[ARDUINO] TLR POS VALUE: " + String(value.toFloat()));
    T_LR_pos = onServosValueRcv(value.toFloat());
    write_serial("[ARDUINO] TLR ADJUSTED POS VALUE: " + String(T_LR_pos));
    dirty = true;
    last_command_time = millis();
    }
  else if (key == HEAD_BF) { 
    B_BF_pos = onServosValueRcv(value.toFloat());
    write_serial("HBF");
    dirty = true;
    last_command_time = millis();
    }
  else if (key == HEAD_UD) { 
    B_UD_pos = onServosValueRcv(value.toFloat());
    write_serial("HUD");
    dirty = true;
    last_command_time = millis();
    }
  else if (key == HEAD_LR) { 
    write_serial("HLR");
    B_LR_pos = onServosValueRcv(value.toFloat());
    dirty = true;
    last_command_time = millis();
    }
  else if (key == MOODS_KEY) { 
    onMoodsValueRcv(value.toFloat());
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

// you can only use this with the NON-BLOCKING raspberry "send_serial" method
// otherwise you'll get rubbish
void resend_data() { 
  if (Serial.available() > 0) {
    String current_data = Serial.readStringUntil('\n');
    write_serial("you sent me: " + current_data);
  }
}

void print_msg_feedback() {
  //write_serial("[MESSAGE RECEIVED] - key: " + current_msg.key + " - value: " + current_msg.value + " -> current speeds:: F:" + String(triskarBase.getForwardTarget()) + " S:" + String(triskarBase.getStrafeTarget()) + " A:" + String(triskarBase.getAngularTarget()) + " :: - max longitudinal speed: " + String(_MAX_SPEED) + " :: - max angular speed: " + String(_MAX_ANGULAR));  
}

//Read data from serial
bool read_serial() {
  
  if (Serial.available() > 0) {
    current_data = Serial.readStringUntil('\n');
    write_serial("AVAILABLE " + current_data);
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
  //triskarBase.setSpeedTargets(0, 0, 0);
  //triskarBase.setWheelSpeeds();

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
  B_BF_pos = MIN_ANGLE_HEAD_BEAK_T2;
  B_LR_pos = MIN_ANGLE_HEAD_BEAK_P;
  B_UD_pos = MIN_ANGLE_HEAD_BEAK_T1;
  B_TB_pos = 90;

  T_UD_pos = MIN_ANGLE_TAIL_BEAK_T2;
  T_P1_pos = MIN_ANGLE_TAIL_BEAK_P;
  T_T1_pos = MIN_ANGLE_TAIL_BEAK_T1;
  T_TN_pos = MIN_ANGLE_TAIL_NECK_T;
  T_BF_pos = 90;
  T_LR_pos = MIN_ANGLE_TAIL_BODY_P;

  B_BF_angle = MIN_ANGLE_HEAD_BEAK_T2;
  B_LR_angle = MIN_ANGLE_HEAD_BEAK_P;
  B_UD_angle = MIN_ANGLE_HEAD_BEAK_T1;
  B_TB_angle = 90;

  T_UD_angle = MIN_ANGLE_TAIL_BEAK_T2;
  T_P1_angle = MIN_ANGLE_TAIL_BEAK_P;
  T_T1_angle = MIN_ANGLE_TAIL_BEAK_T1;
  T_TN_angle = MIN_ANGLE_TAIL_NECK_T;
  T_BF_angle = 90;
  T_LR_angle = MIN_ANGLE_TAIL_BODY_P;

  mood_value = 1;

  // initialization flags
  dirty = false;
}

void final_setup() {
  write_serial("[SETUP] internal setup complete");
}

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
    Serial.println("READ MSG");
    isMsg = true;
  }

  // if at least a msg was received, use the updated values
  if (isMsg) {
    write_serial("IS MSG");
    //triskarBase.setWheelSpeeds();

    set_servos_pos();

    canWrite = true;

    last_serial_time = millis();

    isMsg = false;
  }
}

void setup() {
  // 1 start serial and await a bit for warmup;
  Serial.begin(115200);
  delay(200);
  pwm.begin();
  pwm.setPWMFreq(60);  // Analog servos run at ~60 Hz updates
  // 2 initialize variables
  initialize();
  // 3 setup servos
  initialize_servos();

  //Capacitive sensor - setup 
  if (!cap.begin(0x5A)) {
    Serial.println("MPR121 not found, check wiring?");
    while (1);
  }
  Serial.println("MPR121 found!");
  cap.setThresholds(7.5, 4);

  // wrapping up
  final_setup();
  canWrite = false;
  lastWriteTime = millis();

  write_serial(ARDUINO_OK_FLAG);

  Serial.println("READ");
  
  Serial.setTimeout(5000);
  
  write_serial(ARDUINO_OK_FLAG);

  prevcTime = millis();
}

void loop() {
  enter_loop = millis();
  watchdog_tick();

  //Set values to servos
  serial_loop();

  //MOODS
  if(mood_value == 0)
  {
    hysteria_loop();
  }
  else if(mood_value == 2)
  {
    sin_mov();
  }

  senseTouch();

  end_loop = millis() - enter_loop;
  if(end_loop > 3)
    Serial.println("----------------------------------------LOOP DURATION [ms] : "+ String(end_loop));

}
