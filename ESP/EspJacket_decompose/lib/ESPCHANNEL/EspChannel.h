
#include <set>

#include <EspValue.h>
#include <ESPUDP.h>

#include <Wire.h>
#include <MPU6050_light.h>


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
/*MIO*/
const IPAddress baseIP(192, 168, 111, 4); //IP del raspberry!!!

//const IPAddress baseIP(192, 168, 0, 4); //IP del raspberry!!!
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

        /*
        EspChannel(IPAddress staticIP): _networkChannel(staticIP, baseIP, raspPort, 12, 13, 15) {
            
        }*/
        EspChannel(IPAddress staticIP): _networkChannel(staticIP, baseIP, raspPort) {
            
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


        void loop() {
            // 1. check for TCP messages essages
            // - check if a msg is RCV that UPDATES THE CONFIG
            // TODO
            // for now implemented with SERIAL
            get_serial_msg();

            // 2. loop sensor: get readings from sensor
            loop_sensor();

            // 3. loop send: check if it's time to SEND, then send from ALL ACTIVE ESP VALUES all the AVAILABLE DATA
            loop_send();
        }
};


TwoWire I2Cone = TwoWire(0);
TwoWire I2Ctwo = TwoWire(1);
 float xh,yh,zh;
 float xb,yb,zb;
 int t1,t2,t3;
 int motorPin1 = 14;
 int motorPin2 = 25;
 int motorPin3 = 26;
 int motorPin4 = 27;

 MPU6050 mpu1(I2Cone);
 MPU6050 mpu2(I2Ctwo);
 unsigned long timer = 0;

class AccEspChannelHead: public EspChannel {

    private: 

        // ______________________________________________________________________________________________ ACCELEROMETER
        char _message[255];

        struct AccValuesHead {
            double axh, ayh, azh;
        };

        AccValuesHead _acch;
        
        
        int _pinA;
        int _pinB;

        // ______________________________________________________________________________________________ ESP VALUES

        EspValue _axh;
        EspValue _ayh;
        EspValue _azh;
        
        EspValue _espValues[3] = {_axh, _ayh, _azh};
             
        int _numEspValues;
        //float roll=1, pitch=2, yaw=3;

        //float values[3] = {roll, pitch, yaw};
        

    public: 

        //previous tolerance: 0.05f for all
        AccEspChannelHead(IPAddress staticIP, int pinA, int pinB): EspChannel(staticIP), 
            _axh("axh", 0.1f), _ayh("ayh", 0.1f), _azh("azh", 0.1f) {

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


        void setup_acch() {

            Serial.println("SETUP ACCELEROMETER HEAD -------- BEGIN ");

            // Wire.begin(D6, D7);  
           
            Serial.begin(115200);
            pinMode(motorPin1, OUTPUT);
            pinMode(motorPin2, OUTPUT);
            pinMode(motorPin3, OUTPUT);
            pinMode(motorPin4, OUTPUT);
  
  I2Cone.begin(_pinA, _pinB, 100000);
  byte status = mpu1.begin();
  mpu1.setAccConfig(1);
  Serial.print(F("MPU6050 1 status: "));
  Serial.println(status);
  while (status != 0) { } // stop everything if could not connect to MPU6050
  
  Serial.println(F("Calculating offsets, do not move MPU6050"));
  delay(3000);
  mpu1.calcOffsets(); // gyro and accelero
  mpu1.calcGyroOffsets();
  
  Serial.println("Done!\n");
  
  
 

            
                //_espValues[0].setConfig(true, baseIP); //axh
                //_espValues[1].setConfig(true, baseIP); //ayh
                //_espValues[2].setConfig(true, baseIP); //azh
             
            Serial.println("SETUP TWO ACCELEROMETER HEAD -------- COMPLETE\n");
        }

        void setup_sensor() {
            setup_acch();
        }


        void loop_acch() {

  mpu1.update();
 if ((millis() - timer) > 10) { // print data every 10ms
     Serial.print("Roll : ");
     //roll=map(mpu.getAngleX(),0,50,0,90);
     //roll=mpu.getAngleX();
     xh= mpu1.getAccX();
     Serial.print(xh);
     Serial.print("\tPitch : ");
     //pitch=map(mpu.getAngleY(),0,50,0,90);
     //pitch=mpu.getAngleY();
     yh= mpu1.getAccY();
     //yaw= mpu.getAngleZ();
     Serial.print(yh);
     zh= mpu1.getAccZ();
     Serial.print("\tYaw : ");
     Serial.println(zh);
     timer = millis();

    if (zh<0.4 && zh>-0.4){
        zh=0;
    }

    if (yh<0.4 && yh>-0.4){
        yh=0;
    }

     if(zh<-0.4){
       analogWrite(motorPin1, 100);
     }
     else analogWrite(motorPin1, 0);
     if(yh>0.4){
       analogWrite(motorPin2, 100);
     }
     else analogWrite(motorPin2, 0);
     if(zh>0.4){
       analogWrite(motorPin3, 100);
     }
       else analogWrite(motorPin3, 0);
     if(yh<-0.4){
       analogWrite(motorPin4, 100);
     }
      else analogWrite(motorPin4, 0);
   }
    _acch.axh = -zh;
    _acch.ayh = yh;
    _acch.azh = xh;

                _espValues[0].onNewValueReceived(_acch.axh);
                _espValues[1].onNewValueReceived(_acch.ayh);
                _espValues[2].onNewValueReceived(_acch.azh);
               
                

                /*
                _espValues[0].setConfig(true, baseIP);
                _espValues[1].setConfig(true, baseIP);
                _espValues[2].setConfig(true, baseIP);*/
               


                
               
            }

        void loop_sensor() {
            // Serial.println("CIAO SENSOR --  begin");
            loop_acch();
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

class AccEspChannelBody: public EspChannel {

    private: 

        // ______________________________________________________________________________________________ ACCELEROMETER
        char _message[255];

        struct AccValuesBody {
            double axb, ayb, azb;
        };

        AccValuesBody _accb;
        
        int _pinA;
        int _pinB;

        // ______________________________________________________________________________________________ ESP VALUES

        EspValue _axb;
        EspValue _ayb;
        EspValue _azb;
        
        EspValue _espValues[3] = {_axb, _ayb, _azb};
             
        int _numEspValues;
        //float roll=1, pitch=2, yaw=3;

        //float values[3] = {roll, pitch, yaw};
        

    public: 

        //previous tolerance: 0.05f for all
        AccEspChannelBody(IPAddress staticIP, int pinA, int pinB): EspChannel(staticIP), 
            _axb("axb", 0.1f), _ayb("ayb", 0.1f), _azb("azb", 0.1f) {

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


        void setup_accb() {

            Serial.println("SETUP ACCELEROMETER BODY -------- BEGIN ");

            // Wire.begin(D6, D7);  
           
            Serial.begin(115200);
            
  
  I2Ctwo.begin(_pinA, _pinB, 100000);
  byte status = mpu2.begin();
  mpu2.setAccConfig(2);
  Serial.print(F("MPU6050 2 status: "));
  Serial.println(status);
  while (status != 0) { } // stop everything if could not connect to MPU6050
  
  Serial.println(F("Calculating offsets, do not move MPU6050"));
  delay(3000);
  mpu2.calcOffsets(); // gyro and accelero
  mpu2.calcGyroOffsets();
  
  Serial.println("Done!\n");
  
  
 

            
                _espValues[0].setConfig(true, baseIP); //axb
                _espValues[1].setConfig(true, baseIP); //ayb
                _espValues[2].setConfig(true, baseIP); //azb
                
            Serial.println("SETUP TWO ACCELEROMETER BODY -------- COMPLETE\n");
        }

        void setup_sensor() {
            setup_accb();
        }


        void loop_accb() {

  mpu2.update();
 if ((millis() - timer) > 10) { // print data every 10ms
     Serial.print("Roll : ");
     //roll=map(mpu.getAngleX(),0,50,0,90);
     //roll=mpu.getAngleX();
     xb= mpu2.getAccX();
     Serial.print(xb);
     Serial.print("\tPitch : ");
     //pitch=map(mpu.getAngleY(),0,50,0,90);
     //pitch=mpu.getAngleY();
     yb= mpu2.getAccY();
     //yaw= mpu.getAngleZ();
     Serial.print(yb);
     zb= mpu2.getAccZ();
     Serial.print("\tYaw : ");
     Serial.println(zb);
     timer = millis();

/*
    if (zb<0.1 && zb>-0.1){
        zb=0;
    }

    if (xb<0.4 && xb>-0.4){
        xb=0;
    }
    */

   }
    _accb.axb = xb;
    _accb.ayb = zb;
    _accb.azb = yb;

                _espValues[3].onNewValueReceived(_accb.axb);
                _espValues[4].onNewValueReceived(_accb.ayb);
                _espValues[5].onNewValueReceived(_accb.azb);
               
                
                Serial.print("xb: ");
                Serial.print(xb);
                Serial.print(" yb: ");
                Serial.print(yb);
                Serial.print(" zb: ");
                Serial.println(zb);
                /*
                _espValues[0].setConfig(true, baseIP);
                _espValues[1].setConfig(true, baseIP);
                _espValues[2].setConfig(true, baseIP);*/
              
            }


        void loop_sensor() {
            // Serial.println("CIAO SENSOR --  begin");
            loop_accb();
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





class TouchEspChannel: public EspChannel {

    private: 

        // ______________________________________________________________________________________________ MPX

        // const int trigPin = 5;
        // const int echoPin = 18;

        // //define sound speed in cm/uS
        // #define SOUND_SPEED 0.034
        // #define CM_TO_INCH 0.393701

        // long duration;
        
        //float _distanceCm;
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

        double touch1, touch2, touch3;

        uint8_t _pinA;
        uint8_t _pinB;
        uint8_t _pinC;



        // ______________________________________________________________________________________________ TOUCH

        EspValue _touch1;
        EspValue _touch2;
        EspValue _touch3;
        
        EspValue _espValues[3] = {_touch1, _touch2, _touch3};
             
        int _numEspValues;


    public: 

        TouchEspChannel(IPAddress staticIP, uint8_t pinA, uint8_t pinB, uint8_t pinC): EspChannel(staticIP), 
            _touch1("t1", 0.1f), _touch2("t2", 0.1f), _touch3("t3", 0.1f) {

            _pinA = pinA;
            _pinB = pinB;
            _pinC = pinC;
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


        /*
        void onNewConfigSet(IPAddress destinationIp, char* key, bool set) {
            if (_mpx.keyEquals(key)) {
                _mpx.setConfig(set, destinationIp);
            }
        }*/


        void setup_touch() {
            pinMode(_pinA, INPUT);
            pinMode(_pinB, INPUT);
            pinMode(_pinC, INPUT);
            _espValues[0].setConfig(true, baseIP);
            _espValues[1].setConfig(true, baseIP);
            _espValues[2].setConfig(true, baseIP);
        }
        

        void setup_sensor() {
            setup_touch();
        }


        void loop_touch() {
            touch1 = touchRead(_pinA);
            touch2 = touchRead(_pinB);
            touch3 = touchRead(_pinC);

            
           /*
            curr1 = touch1;
            curr2 = touch2;
            curr3 = touch3;

            

            if (curr1 < prev1 -3 && prev1!=0){
                touch1=1;
            } else {
                touch1=0;
            }

            if (curr2 < prev2 -4 && prev2!=0){
                touch2=1;
            } else {
                touch2=0;
            }

            if (curr3 < prev3 -4 && prev3!=0){
                touch3=1;
            } else {
                touch3=0;
            }
        
            prev1 = curr1;
            prev2 = curr2;
            prev3 = curr3;
            
            */
           if(touch1>21){
            touch1=0;
           }
           if(touch2>31){
            touch2=0;
           }
           if(touch3>38){
            touch3=0;
           }

            // Calcola il valore di pressione in kPa
           
                //_mpx.onNewValueReceived(pressure);
                _espValues[0].onNewValueReceived(touch1);
                _espValues[1].onNewValueReceived(touch2);
                _espValues[2].onNewValueReceived(touch3);
               

        }

        void loop_sensor() {
            loop_touch();
        }


        void loop_send() {
            unsigned long now = millis();
            if (now - _last_send_time > min_send_frequency) {
                bool sent = write_sensors_multi_value(_espValues, _numEspValues);
                Serial.print(touch1); Serial.print(" ,");
                Serial.print(touch2); Serial.print(" ,");
                Serial.print(touch3); Serial.println(" ,");

                if (sent) 
                    _last_send_time = now;
            }
            
        }
};


