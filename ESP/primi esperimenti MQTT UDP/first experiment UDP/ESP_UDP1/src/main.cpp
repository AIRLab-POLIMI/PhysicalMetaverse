#include <Arduino.h>
#include <ESPUDP.h>

#include <MPU6050_tockn.h>
#include <Wire.h>

//GLOBALS


// the current value of the string reading
uint32_t current_value = 0;

// MSGs
char presentation_msg[] = "hallo";
char RASP_AKNOWLEDGE[IN_SIZE] = "OK";  // added last value for convention, which will also be in the "in_packet"
char RESET_MSG[IN_SIZE] = "RESET";

//NETWORKING
const IPAddress staticIP(192, 168, 185, 140);  // this device static IP

const IPAddress defaultDestinationIP(192, 168, 185, 162);  // RASP IP 
const int raspPort = 52108; //44444;  // my mac os udp port is: 49242
 
// last three inputs are the LED PINS
EspUdp espUdp(
    staticIP,
    defaultDestinationIP,
    raspPort,
    12, 
    13, 
    15
);

void checkReset() {
    if (espUdp.udp_msg_equals_to(RESET_MSG)) {

      Serial.println("[checkReset] - received RESET message: '" + String(RESET_MSG) + "'.\n... RESETTING ...");
      ESP.restart();
    }
}

//ACCELEROMETER

MPU6050 mpu6050(Wire);

long timer = 0;
unsigned long t1, dt;
float x,y,z;
int sample_rate=200;

/* 
// ______________________________________________________________________________________________ENCODER
const uint8_t encoderPinA = 4;  // outputA digital GPIO4 - D2
const uint8_t encoderPinB = 5;  // outoutB digital GPIO5 - D1
const uint8_t encoderSW = 14;   // SW (bottone) connesso al pin GPI14 - D5

int encoderCount = 0; // Contatore
int actCLKState; // Lettura attuale del canale CLK (A)
int prevCLKState; // Lettura precedente del canale CLK (A)

#define readA digitalRead(encoderPinA)
#define readB digitalRead(encoderPinB)

ICACHE_RAM_ATTR void CLKChanged() {
  
  int actCLKState = readA;// Leggo il canale A (CLK)

  // Questo if serve per gestire chiamate multiple alla routine di interrupt 
  // causate dal cosiddetto bouncing: ogni volta che si ruota l'albero vengono 
  // in realtà generate diverse variazioni (per ognuna viene scatenato
  // l'interrupt!), dovute al funzionamento meccanico del rotore. Si possono 
  // determinare effetti indesiderati come ad esempio la ripetizione di numeri 
  // ma con questo IF vengono evitati.
  if (prevCLKState != actCLKState) {
    
      encoderCount += (actCLKState == readB ? 1 : -1); 
      
      // Serial.println(encoderCount);
      
      espUdp.write_int_udp(encoderCount);
       
      prevCLKState = actCLKState;
    }
}

// ICACHE_RAM_ATTR void SWPressed() {
  
//   Serial.println("SW Pressed!");
// }

void setup_encoder(){
  
  pinMode(encoderPinA, INPUT); 
  pinMode(encoderPinB, INPUT);
  pinMode(encoderSW, INPUT_PULLUP);
  
  attachInterrupt(digitalPinToInterrupt(encoderPinA), CLKChanged, CHANGE);
  // attachInterrupt(digitalPinToInterrupt(encoderSW), SWPressed, FALLING);

  prevCLKState = readA;  
}
*/
// PINS scl=22, sda=21, gnd, vcc=3.3v


void setup_acc(){
  
  mpu6050.begin();
  mpu6050.calcGyroOffsets(true); //secondi per calibrare
}

void setup() {
  // put your setup code here, to run once:
  // SERIAL
  Serial.begin(115220);
  delay(200);
  Serial.println("SETUP --- --- --- BEGIN");

  // ESP UDP
  espUdp.setup();
  
  // setup acc
  delay(200);
  Wire.begin();
  setup_acc();

  // wait for a few seconds before starting
  delay(100);
  Serial.println("SETUP --- --- --- COMPLETE");
}

void loop() {
  // put your main code here, to run repeatedly:
  // 1. check for UDP essages
  // - check if RESET MSG is received
  noInterrupts();
  if (espUdp.read_udp_non_blocking())
    checkReset();
  interrupts();
  mpu6050.update();
  dt = millis() -t1;
  
  if(dt> sample_rate) {
   t1= millis();
   x = (int)mpu6050.getAngleX();
   y = mpu6050.getAngleY();
   z = mpu6050.getAngleZ();
  }
  espUdp.write_int_udp(x);
  // interrupts can work passively here
  delay(500);
}