
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

//const IPAddress staticIP(192, 168, 0, 30);  // Accelerometer 4
//const IPAddress staticIP(192, 168, 0, 50);  // capsensor
//const IPAddress staticIP(192, 168, 0, 55);  // btn1 - right verde
//const IPAddress staticIP(192, 168, 0, 56);  // btn2 - right blu
//const IPAddress staticIP(192, 168, 0, 57);  // btn3 - wrong rosso
//const IPAddress staticIP(192, 168, 0, 58);  // btn4 - right giallo
const IPAddress staticIP(192, 168, 0, 59);  // btn5 - wrong nero
//const IPAddress staticIP(192, 168, 0, 60);  // btn6 - wrong verde
//const IPAddress staticIP(192, 168, 0, 61);  // btn1 - wrong giallo

//const IPAddress staticIP(192, 168, 43, 10); //btn1 su Honor 9 Lite

// ______________________________________________________________________________________________ ESP CHANNEL


// ---------- ACCELEROMETER
//Accelerometer 4 : SDA 18 SCL 19
//Accelerometer 6 : SDA 21 SCL 22

int accPinA = 18; //18 SDA 32 21
int accPinB = 19; //19 SCL 33 22
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