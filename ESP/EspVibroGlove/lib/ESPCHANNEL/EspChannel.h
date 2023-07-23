
#include <set>

#include <EspValue.h>
#include <ESPUDP.h>

#include <Wire.h>



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
/*MIO
const IPAddress baseIP(192, 168, 111, 4); //IP del raspberry!!!
*/
const IPAddress baseIP(192, 168, 0, 4); //IP del raspberry!!!
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



class FlexEspChannel: public EspChannel {

    private: 

        // ______________________________________________________________________________________________ FLEX
        char _message[255];
        int _pinA;
        int _pinB;
        int _pinC;
        int _pinD;
        #define NUM 4
int arr1[NUM];
int arr2[NUM];
int arr3[NUM];
int arr4[NUM];

int f1=34;
int f2=35;
int f3=32;
int f4=33;

int m1=14;
int m2=27;
int m3=26;
int m4=25;

float media1=0;
int ind1 =0;
int a1=0;
int value1=0;

float media2=0;
int ind2 =0;
int a2=0;
int value2=0;

float media3=0;
int ind3 =0;
int a3=0;
int value3=0;

float media4=0;
int ind4 =0;
int a4=0;
int value4=0;

        
        // ______________________________________________________________________________________________ ESP VALUES

        EspValue _flex1;
        EspValue _flex2;
        EspValue _flex3;
        EspValue _flex4;
        
        EspValue _espValues[4] = {_flex1, _flex2, _flex3, _flex4};
             
        int _numEspValues;
        //float roll=1, pitch=2, yaw=3;

        //float values[3] = {roll, pitch, yaw};
        

    public: 

        //previous tolerance: 0.05f for all
        FlexEspChannel(IPAddress staticIP, int pinA, int pinB, int pinC, int pinD): EspChannel(staticIP), 
            _flex1("f1", 0.1f), _flex2("f2", 0.1f), _flex3("f3", 0.1f), _flex4("f4", 0.1f) {

            _pinA = pinA;
            _pinB = pinB;
            _pinC = pinC;
            _pinD = pinD;


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


        void setup_flex() {

            Serial.println("SETUP Flex -------- BEGIN ");
  pinMode(f1, INPUT);
  pinMode(f2, INPUT);
  pinMode(f3, INPUT);
  pinMode(f4, INPUT);
  pinMode(m1, OUTPUT);
  pinMode(m2, OUTPUT);
  pinMode(m3, OUTPUT);
  pinMode(m4, OUTPUT);
  
  for (int i=0; i<NUM; i++){
    arr1[i]= analogRead(f1);
    arr2[i]= analogRead(f2);
    arr3[i]= analogRead(f3);
    arr4[i]= analogRead(f4);
    media1 += arr1[i];
    media2 += arr2[i];
    media3 += arr3[i];
    media4 += arr4[i];
  }
  media1 = media1/NUM;
  media2 = media2/NUM;
  media3 = media3/NUM;
  media4 = media4/NUM;
            
            
                //_espValues[0].setConfig(true, baseIP); //flex1
                _espValues[1].setConfig(true, baseIP); //flex2
                //_espValues[2].setConfig(true, baseIP); //flex3
                _espValues[3].setConfig(true, baseIP); //flex4
            Serial.println("SETUP flex -------- COMPLETE\n");
        }

        void setup_sensor() {
            setup_flex();
        }


        void loop_flex() {

                a1 = arr1[ind1];
  arr1[ind1] = analogRead(f1);
  media1 = (media1*NUM - a1 + arr1[ind1])/NUM;
  ind1 += 1;
  if (ind1 >= NUM){
    ind1=0;
  }

  value1= map(media1, 0, 20, 0, 255);
  if(value1>255){
    value1=255;
  }
  if(value1<0){
    value1=0;
  }
//---------------------2----------------
  a2 = arr2[ind2];
  arr2[ind2] = analogRead(f2);
  media2 = (media2*NUM - a2 + arr2[ind2])/NUM;
  ind2 += 1;
  if (ind2 >= NUM){
    ind2=0;
  }

  value2= map(media2, 0, 20, 0, 255);
  if(value2>255){
    value2=255;
  }
  if(value2<0){
    value2=0;
  }
//-----------------------3-----------------
  a3 = arr3[ind3];
  arr3[ind3] = analogRead(f3);
  media3 = (media3*NUM - a3 + arr3[ind3])/NUM;
  ind3 += 1;
  if (ind3 >= NUM){
    ind3=0;
  }

  value3= map(media3, 0, 20, 0, 255);
  if(value3>255){
    value3=255;
  }
  if(value3<0){
    value3=0;
  }
//----------------------4-----------------
  a4 = arr4[ind4];
  arr4[ind4] = analogRead(f4);
  media4 = (media4*NUM - a4 + arr4[ind4])/NUM;
  ind4 += 1;
  if (ind4 >= NUM){
    ind4=0;
  }

  value4= map(media4, 0, 20, 0, 255);
  if(value4>255){
    value4=255;
  }
  if(value4<0){
    value4=0;
  }
  
  Serial.print(analogRead(f1));
  Serial.print (" ");
  Serial.println(value1);
  analogWrite(m1, value1);
  //delay(50);
  
  
  Serial.print(analogRead(f2));
  Serial.print (" ");
  Serial.println(value2);
  analogWrite(m2, value2);
  //delay(50);
  
  
  Serial.print(analogRead(f3));
  Serial.print (" ");
  Serial.println(value3);
  analogWrite(m3, value3);
  //delay(50);
  

  Serial.print(analogRead(f4));
  Serial.print (" ");
  Serial.println(value4);
  analogWrite(m4, value4);
  //delay(50);
                _espValues[0].onNewValueReceived(value1);
                _espValues[1].onNewValueReceived(value2);
                _espValues[2].onNewValueReceived(value3);
                _espValues[3].onNewValueReceived(value4);
                

                
            }
        

        void loop_sensor() {
            // Serial.println("CIAO SENSOR --  begin");
            loop_flex();
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


