
#include <Arduino.h>
#include <EspChannel.h> 



// ______________________________________________________________________________________________ PARAMETERS

//Static IP address configuration
//      ROBOT IPS
// IP = 192.168.0.6 -> SIID
// IP = 192.168.0.7 -> BLACKWING
// IP = 192.168.0.8 -> SONOMA
//      ESP IPS
// IP = 192.168.0.40 -> ACC 1
// IP = 192.168.0.41 -> ACC 2
// IP = 192.168.0.50 -> SONAR 1

const IPAddress staticIP(192, 168, 0, 90);  // this device static IP
// const IPAddress staticIP(192, 168, 0, 40);  // this device static IP


// ______________________________________________________________________________________________ ESP CHANNEL


// ---------- ACCELEROMETER

int fPinA = 34; 
int fPinB = 35; 
int fPinC = 32; 
int fPinD = 33; 
FlexEspChannel espChannel(staticIP, fPinA, fPinB, fPinC, fPinD);




// ______________________________________________________________________________________________ MAIN

void setup() {

  // SERIAL
  Serial.begin(115200);
  delay(200);
  Serial.println("SETUP --- --- --- BEGIN");

  // SETUP ESP CHANNEL
  espChannel.setup();
  delay(200);

  // wait for a few seconds before starting
  delay(100);
  Serial.println("SETUP --- --- --- COMPLETE");
}

void loop() {

  espChannel.loop();
}