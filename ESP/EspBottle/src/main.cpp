
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

const IPAddress staticIP(192, 168, 111, 40);  // this device static IP
// const IPAddress staticIP(192, 168, 0, 40);  // this device static IP


// ______________________________________________________________________________________________ ESP CHANNEL


// ---------- ACCELEROMETER

int accPinA = 21; //18 SDA
int accPinB = 22; //19 SCL
AccEspChannel espChannel(staticIP, accPinA, accPinB);




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