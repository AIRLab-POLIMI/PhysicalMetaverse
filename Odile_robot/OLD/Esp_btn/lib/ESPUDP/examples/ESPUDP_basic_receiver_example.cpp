
#include <Arduino.h>
#include <ESPUDP.h>


// ______________________________________________________________________________________________ESP UDP

// // ******** IP - MAPPINGS
// // *** YOUR PROJECT
// // IP = 192.168.1.60 -> THIS DEVICE
// // IP = 192.168.1.61 -> OTHER DEVICE (the one you are comm with through UDP)

IPAddress staticIP(192, 168, 1, 60);  // this device static IP
IPAddress defaultDestinationIP(192, 168, 1, 2);       
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


bool getUDPMessage() {

    // MAIN "RECEIVE" method, used in the LOOP. 
    // check if there is a VALID UDP message: either a PURE MESSAGE or a KEY-VALUe message. 
    // in that case return true, false otherwise.

    // 1 : check if there is an incoming UDP message.
    //     if there is none, return false; otherwise proceed. 
    if (!espUdp.read_udp_non_blocking())
        return false;

    // 2 : check if the incoming UDP message is a PURE MESSAGE. 
    //     if it is, the response is called directly from the "checkPureMessages" method, which will
    //     in that case return true; otherwise proceed.
    if (checkPureMessages())
        return true;

    // 3 : check if the incoming UDP message is a KEY-VALUE MESSAGE. 
    //     if it is, the response is called directly from the "checkKeyValueMessages" method, which will
    //     in that case return true; otherwise return false 
    //     (which are the return values of "checkKeyValueMessages" for these two cases)
    return checkKeyValueMessages();
}


// ______________________________________________________________________________________________MAIN

void setup_variables() {
    // setup your variables here
}

void setup()
{
    // serial setup 
    Serial.begin(115200);
    delay(200);
    Serial.println("[SETUP] ______________________ BEGIN");

    // setup variables
    setup_variables();

    // SETUP ESP UDP (leds + UDP)
    espUdp.setup();

    // setup completed
    Serial.println("[SETUP] ______________________ COMPLETE");
}

void loop() 
{
    // check if there is a valid UDP message and in that case respond to it and return true. 
    // Otherwise return false;
    bool isValidMsg = getUDPMessage();

    // if a UDP message was received in general, respond to the sender.
    // the second parameter "true" means that the msg will be a RESPONSE: sent back to the last sender. 
    // if it were FALSE, the msg would have been sent to the DEFAULT DESTINATION;
    // to specify a destination (ip, port), use the corresponsing method overload with three inputs (char *, IPAddress, int)
    if (espUdp.received) 
      espUdp.write_char_udp("thanks for the message!", true);

    // you can add a DELAY if you don't want to force high frequency check and eventual response
    // delay (50);
}