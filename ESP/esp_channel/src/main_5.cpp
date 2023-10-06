
#include <Arduino.h>
#include <ESPUDP.h>


// ______________________________________________________________________________________________ESP UDP

// // ******** IP - MAPPINGS
// // *** YOUR PROJECT
// // IP = 192.168.1.60 -> THIS DEVICE
// // IP = 192.168.1.61 -> OTHER DEVICE (the one you are comm with through UDP)

IPAddress staticIP(192, 168, 0, 60);  // this device static IP
IPAddress defaultDestinationIP(192, 168, 0, 104);       
                                      // other device IP
                                      // NB you can have as many "other IPs" as you want; 
                                      // the one you pass in the ESPUDP constructor is just the default. 
                                      // You can still send data by passing different IPAddress ip's 
                                      // as a parameter to the SEND method

// int defaultDestinationPort = 2566;  // NB the default destination port is the same as the ESP's OWN port, 
                                    // because the most common use case is for ESPs to communicate with each other. 
                                    // However, if you want a different defaultDestinationPort, just pass this as input to the ESPUDP constructor. 
                                    // as the third parameter.
                                    //
                                    //  EspUdp espUdp(
                                    //      staticIP,
                                    //      defaultDestinationIP,
                                    //      defaultDestinationPort
                                    //  );
                                    //
                                    // remember: you can still send to different receivers than the default one! The default one is a utility
                                    // to use in case there is a main receiver and you want the ESPUDP object to communicate with it wihtout
                                    // having to pass its IP and PORT explicitly every time. 


// COMMON MESSAGES 
#define MSG_1 "MSG_1"
#define MSG_2 "MSG_2"

// COMMON KEYS (keys of UDP key-value msgs that you want to respond to)
std::string KEY_1 = "KEY1";
std::string KEY_2 = "KEY2";

const uint8_t green_pin = 27;
const uint8_t red_pin = 12;
const uint8_t blue_pin = 26;

const bool CORRECT_STATION = false;

// define the ESPUDP object. 
// Depending on the device (either ESP32 or ESP8266), 
// it will use different dependancies and implement some methods slightly differently
EspUdp espUdp(
    staticIP,
    defaultDestinationIP
);


// ______________________________________________________________________________________________RESPONSES

// write here the resposes to UDP messages
// - you can have custom responses to specific KEYS of KEY-VALUE messages;
// - you can have custom responses to specific MESSAGES; 
//
// the suggested way to do so in the LOOP, if the "pure messages" are going to arrive 
// at lower frequency than the KEY-VALUE messages, 
// is the following:
// * check if some data has arrived though UDP
//    |-- if it has: 
//         * check if they are "pure message", and in that case respond and terminate UDP check
//         * if they are NOT "pure messages", check if they are key-value messages, with a key that is recognized
//           |-- if a key is recognized: 
//              * respond to that KEY-VALUE message


void respondToMsg1() {
    // method called if the received UDP msg was a PURE MSG = MSG1.
    // respond to MSG1 received via UDP
    Serial.println("Responding to MSG1 received via UDP!");
}

void respondToMsg2() {
    // method called if the received UDP msg was a PURE MSG = MSG1.
    // respond to MSG1 received via UDP
    Serial.println("Responding to MSG1 received via UDP!");
}

bool checkPureMessages()
{
    bool isPureMsg = false;

    if (espUdp.udp_msg_equals_to(MSG_1)) {
        respondToMsg1();
        isPureMsg = true;
    }
    if (espUdp.udp_msg_equals_to(MSG_2)) {
        respondToMsg2();
        isPureMsg = true;
    }

    return isPureMsg;
}

void respondToKey1() {
    // method called if the received UDP msg was a KEY-VALUE msg with KEY = KEY1.
    // you can do something with KeyValueMSG. e.g:

    Serial.print("Received KEY 1 MSG: KEY: '");
    Serial.print(espUdp.key().c_str());
    Serial.print("' with VALUE: ");
    Serial.println(espUdp.val().c_str());
}

void respondToKey2() {
    // method called if the received UDP msg was a KEY-VALUE msg with KEY = KEY1.
    // you can do something with KeyValueMSG. e.g:
    
    Serial.print("Received KEY 2 MSG: KEY: '");
    Serial.print(espUdp.key().c_str());
    Serial.print("' with VALUE: ");
    Serial.println(espUdp.val().c_str());
}

bool checkKeyValueMessages()
{
    bool isKeyValueMsg = false;

    espUdp.getKeyValue();

    Serial.print("[KEY VALUE]: K: '");
    Serial.print(espUdp.key().c_str());
    Serial.print("' - V: '");
    Serial.print(espUdp.val().c_str());
    Serial.print("' - float V: '");
    Serial.print(espUdp.floatValue());
    Serial.println("'");

    if (espUdp.isKeyEqualTo(KEY_1)) {
        respondToKey1();
        isKeyValueMsg = true;
    }

    if (espUdp.isKeyEqualTo(KEY_2)) {
        respondToKey2();
        isKeyValueMsg = true;
    }

    return isKeyValueMsg;
}




//bool toggle pin
bool togglePin = false;
//enum
enum { IDLE, BLINKING, ACTIVATED } blinking_state;

void setup_variables();

bool getUDPMessage() {

    // MAIN "RECEIVE" method, used in the LOOP. 
    // check if there is a VALID UDP message: either a PURE MESSAGE or a KEY-VALUe message. 
    // in that case return true, false otherwise.

    // 1 : check if there is an incoming UDP message.
    //     if there is none, return false; otherwise proceed. 
    if (!espUdp.read_udp_non_blocking())
        return false;

    //if received value is "BLINK"
    if (espUdp.udp_msg_equals_to("BLINK")) {
        Serial.println("BLINK");
        if (blinking_state == IDLE) {
            blinking_state = BLINKING;
        }
    }

    //if received value is "RESET"
    if (espUdp.udp_msg_equals_to("RESET")) {
        Serial.println("RESET");
        setup_variables();
    }

    togglePin = !togglePin;
    //turn on led 27
    /*
    if (togglePin) {
        digitalWrite(green_pin, HIGH);
        digitalWrite(red_pin, LOW);
    }
    else {
        digitalWrite(green_pin, LOW);
        digitalWrite(red_pin, HIGH);
    }
    */

    // 3 : check if the incoming UDP message is a KEY-VALUE MESSAGE. 
    //     if it is, the response is called directly from the "checkKeyValueMessages" method, which will
    //     in that case return true; otherwise return false 
    //     (which are the return values of "checkKeyValueMessages" for these two cases)
    return checkKeyValueMessages();
}

int blink_amount = 12;

void blink() {
    if (CORRECT_STATION) {
        //blink green pin every 300ms for 4 times
        for (int i = 0; i < blink_amount; i++) {
            digitalWrite(green_pin, HIGH);
            delay(300);
            digitalWrite(green_pin, LOW);
            delay(300);
        }
    }
    else {
        //blink red pin every 300ms for 4 times
        for (int i = 0; i < blink_amount; i++) {
            digitalWrite(red_pin, HIGH);
            delay(300);
            digitalWrite(red_pin, LOW);
            delay(300);
        }
    }
    //set blinking state to ACTIVATED
    blinking_state = ACTIVATED;
    //turn off red and green
    digitalWrite(red_pin, LOW);
    digitalWrite(green_pin, LOW);
    //turn on blue pin
    digitalWrite(blue_pin, LOW);
}


// ______________________________________________________________________________________________MAIN

void setup_variables() {
    // setup your variables here
    // turn off all leds
    pinMode(green_pin, OUTPUT);
    pinMode(red_pin, OUTPUT);
    pinMode(blue_pin, OUTPUT);
    digitalWrite(green_pin, LOW);
    digitalWrite(red_pin, LOW);
    digitalWrite(blue_pin, LOW);
    //turn on green if correct else
    if (CORRECT_STATION) {
        digitalWrite(green_pin, HIGH);
    }
    else {
        digitalWrite(red_pin, HIGH);
    }
    blinking_state = IDLE;
}

void setup()
{
    // serial setup 
    Serial.begin(115200);
    delay(200);
    Serial.println("[SETUP] ______________________ BEGIN");

    // SETUP ESP UDP (leds + UDP)
    espUdp.setup();

    // setup completed
    Serial.println("[SETUP] ______________________ COMPLETE");

    // setup variables
    setup_variables();

}

void loop() 
{
    // check if there is a valid UDP message and in that case respond to it and return true. 
    // Otherwise return false;
    bool isValidMsg = getUDPMessage();

    //case
    switch (blinking_state) {
    case ACTIVATED:
        break;
    case IDLE:
        break;
    case BLINKING:
        //blink
        blink();
        break;
    }
    //wait 100ms
    delay(100);
    // you can add a DELAY if you don't want to force high frequency check and eventual response
    // delay (50);
}