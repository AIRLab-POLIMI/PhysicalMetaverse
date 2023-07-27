
#include <set>
#include <Arduino.h>

#include <EspValue.h>
#include <ESPUDP.h>
#include <MPU6050_light.h>
#include <Wire.h>

//#include "I2Cdev.h"
//#include "MPU6050_6Axis_MotionApps20.h"
//#include "MPU6050_6Axis_MotionApps612.h"


// ______________________________________________________________________________________________ CONSTANTS

// comm defaults
#define DELIMITER ":"
#define MSG_DELIMITER "_"
char RESET_MSG[IN_SIZE] = "RESET";

/*
const IPAddress siidIP(192, 168, 0, 3); 
const IPAddress blackwingIP(192, 168, 0, 2);
const IPAddress memoriaIP(192, 168, 0, 4);
IPAddress destinationIPs [] = {siidIP, blackwingIP, memoriaIP}; 
*/


//const IPAddress baseIP(192, 168, 0, 4); //IP del raspberry!!!
const IPAddress baseIP(192, 168, 0, 3); //IP del raspberry!!!
IPAddress destinationIPs [] = {baseIP};

const int raspPort = 44444;  // my mac os udp port is: 49242
//const int raspPort = 40616;
const unsigned int min_send_frequency = 10;  // how many milliseconds MUST elapse AT LEAST between two consecutive calls to SEND


// ______________________________________________________________________________________________ ESP CHANNEL


class EspChannel {

    protected: 
    
        EspUdp _networkChannel;

        unsigned long _last_send_time;

        void sendToRobot(String msg, IPAddress destinationIP) {
            
            _networkChannel.write_String_udp(msg, destinationIP, raspPort); 

            /*
            Serial.print("[EspChannel][sendToRobot] --- Sending msg: '");
            Serial.print(msg);
            Serial.print("' - to destination IP: '");
            Serial.print(destinationIP);
            Serial.println("'\n");
            */
        }


    public: 

        EspChannel(IPAddress staticIP): _networkChannel(staticIP, baseIP, raspPort, 12, 13, 15) {
            
        }

        bool write_sensors_single_value(EspValue espValue) {
            
            // IF there is at least one char in msg:
            // - send via udp to destination

            // return TRUE if something was sent.

            if (!espValue.canSend())
                return false;

            String msg = espValue.get_value();
            
            if (msg.length() > 0) {
                sendToRobot(msg, espValue.getDestinationIp()); 
                return true;
            } 
            return false;
        }

        bool write_sensors_multi_value_to_destination(EspValue espValues[], int numEspValues, IPAddress destinationIP) {

            String msg = "";

            // Serial.print("[write_sensors_multi_value_to_destination] - destination IP: ");
            // Serial.println(destinationIP);

            // WRITE ALL ESP VALUES that have a value TO SEND
            for (size_t i = 0; i < numEspValues; i++)
            {
                // if this espValue CAN SEND AND the destination is the current destination, add its message to the msg, if present
                if (espValues[i].canSend() && espValues[i].getDestinationIp() == destinationIP) {
                    
                    String tempNsg = espValues[i].get_key_value();
                    
                    // Serial.print("[write_sensors_multi_value_to_destination] ----- get key value: ");
                    // Serial.print(tempNsg);
                    // Serial.print(" ----- can send: ");
                    // Serial.print(espValues[i].canSend());
                    // Serial.println(" ----- \n");

                    msg += tempNsg;
                    if (tempNsg.length() > 0)
                    msg += MSG_DELIMITER;
                }
            }

            // IF there is at least one char in msg:
            // - remove last delimiter
            // - send via udp to destination
            if (msg.length() > 0) {
                msg.remove(msg.length() - 1);
                sendToRobot(msg, destinationIP);
                Serial.print(msg);
                return true;
            } 
            return false;
        }

        bool write_sensors_multi_value(EspValue espValues[], int numEspValues) {

            // return TRUE if something was sent.

            // there is ONE MESSAGE for EVERY POSSIBLE DESTINATION
            // 1. create an array of ALL the ACTIVE DESTINATIONS
            // Create a set to store unique destinationIPs
            std::set<IPAddress> uniqueIPs;

            // Serial.print("[write_sensors_multi_value] --- numEspValues : ");
            for (size_t i = 0; i < numEspValues; i++)
            {
                // Serial.print("[write_sensors_multi_value] esp value KEY: '");
                // Serial.print(espValues[i].getKey());
                // Serial.print("' - can send: '");
                // Serial.println(espValues[i].canSend());

                if (espValues[i].canSend()) {
                    uniqueIPs.insert(espValues[i].getDestinationIp());
                    // Serial.print("[write_sensors_multi_value] - inserting destination IP: ");
                    // Serial.println(espValues[i].getDestinationIp());
                }
            }
            
            // LOOP over all the DESTINATIONS, and for each BUILD THE CORRESPONDING MESSAGE and send it
            bool hasSent = false;
            for (const auto& ip : uniqueIPs) {
                if (write_sensors_multi_value_to_destination(espValues, numEspValues, ip));
                    hasSent = true;
            }
            return hasSent;
        }


        virtual void onNewConfigSet(IPAddress destinationIP, char* key, bool set) = 0;

    // ______________________________________________________________________________________________ SERIAL COMM


        struct KeyValueMsg {
        String key;
        String value;  
        };

        KeyValueMsg current_msg = {"", ""}; // global variable with the current received message

        void get_key_value(String msg) {
            // 1 find the position of the delimiter
            // 2 get the first and second substrings: the KEY and VALUE of the message

            int delim_index = msg.indexOf(DELIMITER);

            String key = msg.substring(0, delim_index);
            String value = msg.substring(delim_index + 1);

            // current_msg.key = key;
            // current_msg.value = value;

            // convert String key to CHAR*
            // - Length (with one extra character for the null terminator)
            int str_len = key.length() + 1; 
            // - Prepare the character array (the buffer) 
            char char_array[str_len];
            // - Copy it over 
            key.toCharArray(char_array, str_len);

            //KeyValueMsg tempKeyValueMsg = KeyValueMsg();
            //tempKeyValueMsg.key = key;
            //tempKeyValueMsg.value = value;

            bool send;
            
            if (value==String('0'))
                send = false;
            else 
                send = true;
            
            IPAddress destinationIP = baseIP;
            Serial.println(char_array);
            onNewConfigSet(destinationIP, char_array, send);
        }

        String current_data = "";

        bool read_serial() {
            
            if (Serial.available() > 0) {
                current_data = Serial.readStringUntil('\n');
                return true;    
            }
            else
                return false;
        }

        bool get_serial_msg() {
            current_data = "";
            if (read_serial())
                get_key_value(current_data);
                return true;
            return false;
        }

    // ______________________________________________________________________________________________ SERIAL COMM END

        virtual void setup_sensor() = 0;

        void setup() {

            // setup networking channel
            _networkChannel.setup();

            // setup sensor
            setup_sensor();

        }

        virtual void loop_sensor() = 0;


        virtual void loop_send() = 0;

        int _ping = 0;

        

        void loop() {
            if(_ping == 0)
                _ping = 1;
            else{
                int _pinA = 27; //18 SDA
                int _pinB = 26; //19 SCL
                TwoWire I2Cone = TwoWire(0);
                MPU6050 mpu(I2Cone);
                I2Cone.begin(_pinA, _pinB, 100000);
                byte status = mpu.begin();
                mpu.setAccConfig(2);
                Serial.print(F("MPU6050 status: "));
                Serial.println(status);
                while (status != 0) { 
                    I2Cone.begin(_pinA, _pinB, 100000);
                    status = mpu.begin();
                    mpu.setAccConfig(2);
                    Serial.print(F("MPU6050 status: "));
                    Serial.println(status);
                    delay(3000);
                }
            }
            // 1. check for TCP messages essages
            // - check if a msg is RCV that UPDATES THE CONFIG
            // TODO
            // for now implemented with SERIAL
            get_serial_msg();

            // 2. loop sensor: get readings from sensor
            loop_sensor();
            _ping = 0;

            // 3. loop send: check if it's time to SEND, then send from ALL ACTIVE ESP VALUES all the AVAILABLE DATA
            loop_send();
            
        }
};

TwoWire I2Cone = TwoWire(0);
 float x,y,z;
 

 MPU6050 mpu(I2Cone);
 unsigned long timer = 0;

class JAccEspChannel: public EspChannel {

    private: 

        // ______________________________________________________________________________________________ ACCELEROMETER
        char _message[255];
        
        


        struct AccValues {
            double ax, ay, az;
        };

        AccValues _acc;
        
        int _pinA;
        int _pinB;

        // ______________________________________________________________________________________________ ESP VALUES

        EspValue _ax;
        EspValue _ay;
        EspValue _az;
        /*EspValue _roll1;
        EspValue _pitch1;
        EspValue _yaw1;*/
        
        
        EspValue _espValues[3] = {_ax, _ay, _az};
             
        int _numEspValues;
        //float roll=1, pitch=2, yaw=3;

        //float values[3] = {roll, pitch, yaw};
        

    public: 

        //previous tolerance: 0.05f for all
        JAccEspChannel(IPAddress staticIP, int pinA, int pinB): EspChannel(staticIP), 
            _ax("axh", 0.05f), _ay("ayh", 0.05f), _az("azh", 0.05f) {

            _pinA = pinA;
            _pinB = pinB;

            _numEspValues = sizeof(_espValues)/sizeof(_espValues[0]);
        }


        void onNewConfigSet(IPAddress destinationIp, char* key, bool set) {

            // adding a new config means: search over the ESPVALUES, find the one with key='key', and: 
            //
            // if SET is TRUE, we need to ADD a new config. 
            // - set its '_toSend' = True
            // - set its '_destinationIP' = 'destinationIp'
            //
            // if SET is FALSE, we need to REMOVE the config.
            // - set its '_toSend' = False
            // - set its '_destinationIP' = defaultIpAddress 

            // this is already done WHITHIN the "setConfig" method.

            for (size_t i = 0; i < _numEspValues; i++)
            {
                if ( _espValues[i].keyEquals(key)) {
                    // Serial.print("------****************************** YRH: KEY: ");
                    // Serial.println(key);
                     _espValues[i].setConfig(set, destinationIp);
                    
                    /**/
                    Serial.print("[EspChannel][onNewConfigSet] - destinationIP: '");
                    Serial.print(destinationIp);
                    Serial.print("' - key: ");
                    Serial.print(key);
                    Serial.print("' - set: ");
                    Serial.print(set);
                    Serial.print("' - esp val can send: ");
                    Serial.print(_espValues[i].canSend());
                    Serial.println("\n");
                }
            }
        }

        


        void setup_acc() {

            Serial.println("SETUP ACCELEROMENTER -------- BEGIN ");

            // Wire.begin(D6, D7);  
           
            Serial.begin(115200);
            
  
  I2Cone.begin(_pinA, _pinB, 100000);
  byte status = mpu.begin();
  mpu.setAccConfig(2);
  Serial.print(F("MPU6050 status: "));
  Serial.println(status);
  while (status != 0) { 
    I2Cone.begin(_pinA, _pinB, 100000);
    status = mpu.begin();
    mpu.setAccConfig(2);
    Serial.print(F("MPU6050 status: "));
    Serial.println(status);
    delay(3000);
  } // stop everything if could not connect to MPU6050
  /*
  Serial.println(F("Calculating offsets, do not move MPU6050"));
  delay(3000);
  mpu.calcOffsets(); // gyro and accelero
  mpu.calcGyroOffsets();
  */
  Serial.println("Done!\n");
  
  
 

            
                _espValues[0].setConfig(true, baseIP); //ax
                _espValues[1].setConfig(true, baseIP); //ay
                //_espValues[2].setConfig(true, baseIP); //yaw0
                //_espValues[3].setConfig(true, baseIP); //roll1
                //_espValues[4].setConfig(true, baseIP); //pitch1
                //_espValues[5].setConfig(true, baseIP); //yaw1
            Serial.println("SETUP TWO ACCELEROMETER -------- COMPLETE\n");
        }

        void setup_sensor() {
            setup_acc();
        }


        void loop_acc() {

  mpu.update();
 if ((millis() - timer) > 10) { // print data every 10ms
     Serial.print("Roll : ");
     //roll=map(mpu.getAngleX(),0,50,0,90);
     //roll=mpu.getAngleX();
     x= mpu.getAccX();
     Serial.print(x);
     Serial.print("\tPitch : ");
     //pitch=map(mpu.getAngleY(),0,50,0,90);
     //pitch=mpu.getAngleY();
     y= mpu.getAccY();
     //yaw= mpu.getAngleZ();
     Serial.print(y);
     z= mpu.getAccZ();
     Serial.print("\tYaw : ");
     Serial.println(z);
     timer = millis();
    }

    if (x < 0.1 && x> -0.1){
        x=0;
    }
    if (y < 0.05 && y> -0.05){
        y=0;
    }
    if (z < 0.15 && z> -0.15){
        z=0;
    }

    _acc.ax = x;
    _acc.ay = y;
    _acc.az = z;

                _espValues[0].onNewValueReceived(_acc.ax);
                _espValues[1].onNewValueReceived(_acc.ay);
                _espValues[2].onNewValueReceived(_acc.az);
               
                

                /*
                _espValues[0].setConfig(true, baseIP);
                _espValues[1].setConfig(true, baseIP);
                _espValues[2].setConfig(true, baseIP);*/
               


                
               
            }
        

        void loop_sensor() {
            // Serial.println("CIAO SENSOR --  begin");
            loop_acc();
            // Serial.println("CIAO SENSOR --  end");
        }


        void loop_send() {

            // Serial.println("CIAO SEND --  begin");

            unsigned long now = millis();
            if (now - _last_send_time > min_send_frequency) {
                
                // Serial.println(" ---- SEND TIME!");
                bool sent = write_sensors_multi_value(_espValues, _numEspValues);
                if (sent) {
                    _last_send_time = now;
                    // Serial.println("[loop_send] ****** HAS SENT..!");
                }
            }

            // Serial.println("CIAO SEND --  end");
        }
};



class SonarEspChannel: public EspChannel {

    private: 

        // ______________________________________________________________________________________________ SONAR

        // const int trigPin = 5;
        // const int echoPin = 18;

        // //define sound speed in cm/uS
        // #define SOUND_SPEED 0.034
        // #define CM_TO_INCH 0.393701

        // long duration;
        float _distanceCm;
        // float distanceInch;

        // void setupSonar() {
        //   pinMode(trigPin, OUTPUT); // Sets the trigPin as an Output
        //   pinMode(echoPin, INPUT); // Sets the echoPin as an Input
        // }

        // void loopSonar() {
        //   // Clears the trigPin
        //   digitalWrite(trigPin, LOW);
        //   delayMicroseconds(2);
        //   // Sets the trigPin on HIGH state for 10 micro seconds
        //   digitalWrite(trigPin, HIGH);
        //   delayMicroseconds(10);
        //   digitalWrite(trigPin, LOW);
        
        //   // Reads the echoPin, returns the sound wave travel time in microseconds
        //   duration = pulseIn(echoPin, HIGH);
        
        //   // Calculate the distance
        //   distanceCm = duration * SOUND_SPEED/2;
        // }


        // int trigPin = 6;      // trigger pin
        // int echoPin = 7;      // echo pin

        // NewPing sonar(trigPin, echoPin);

        uint8_t _pinA;
        uint8_t _pinB;

        // ______________________________________________________________________________________________ SONAR

        EspValue _sonar;


    public: 

        SonarEspChannel(IPAddress staticIP, uint8_t pinA, uint8_t pinB): EspChannel(staticIP), 
            _sonar("s", 2) {

            _pinA = pinA;
            _pinB = _pinB;
        }


        void onNewConfigSet(IPAddress destinationIp, char* key, bool set) {
            if (_sonar.keyEquals(key)) {
                _sonar.setConfig(set, destinationIp);
            }
        }


        void setup_sonar() {}

        void setup_sensor() {
            setup_sonar();
        }


        void loop_sonar() {}

        void loop_sensor() {
            loop_sonar();
        }


        void loop_send() {
            unsigned long now = millis();
            if (now - _last_send_time > min_send_frequency) {
                bool sent = write_sensors_single_value(_sonar);
                if (sent) 
                    _last_send_time = now;
            }
        }
};
