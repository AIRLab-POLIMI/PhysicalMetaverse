
#include <set>
#include <Arduino.h>

#include <EspValue.h>
#include <ESPUDP.h>

#include <Wire.h>

#include "I2Cdev.h"
//#include "MPU6050_6Axis_MotionApps20.h"
#include "MPU6050_6Axis_MotionApps612.h"


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

MPU6050 _mpu68(0x68);
MPU6050 _mpu69(0x69);

class GsrTwoAccEspChannel: public EspChannel {

    private: 

        // ______________________________________________________________________________________________ ACCELEROMETER
        char _message[255];
        
        

        uint8_t error_code = 0U;      // return status after each device operation (0 = success, !0 = error)

        Quaternion _q68;           // [w, x, y, z]         quaternion container
        Quaternion _q69;
        float q68_69[4];
        float data[3];

        struct EulerAngles {
            double roll0, pitch0, yaw0, roll1, pitch1, yaw1;
        };

        EulerAngles _angles;
        //acc
        int _pinA;
        int _pinB;
        //gsr
        int _pinC;

        #define NUM 300
int arr[NUM];
float media=0;
int ind =0;
int a=0;

        // ______________________________________________________________________________________________ ESP VALUES

        EspValue _roll0;
        EspValue _pitch0;
        EspValue _yaw0;
        EspValue _roll1;
        EspValue _pitch1;
        EspValue _yaw1;

        EspValue _gsr;
        
        EspValue _espValues[7] = {_roll0, _pitch0, _yaw0, _roll1, _pitch1, _yaw1, _gsr};
             
        int _numEspValues;
        //float roll=1, pitch=2, yaw=3;

        //float values[3] = {roll, pitch, yaw};
        

    public: 

        //previous tolerance: 0.05f for all
        GsrTwoAccEspChannel(IPAddress staticIP, int pinA, int pinB, int pinC): EspChannel(staticIP), 
            _roll0("ax0", 5.0f), _pitch0("ay0", 5.0f), _yaw0("az0", 5.0f), _roll1("ax1", 5.0f), _pitch1("ay1", 5.0f), _yaw1("az1", 5.0f), _gsr("gsr", 5.0f) {

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

        void inverseMultiplyQuaternion(Quaternion q68, Quaternion q69, float* q68_69){
    

    float x0= -q68.x;
    float y0= -q68.y;
    float z0= -q68.z;
    float w0= q68.w;
    float x1= q69.x;
    float y1= q69.y;
    float z1= q69.z;
    float w1= q69.w;
    

    q68_69[0] = -x1 * x0 - y1 * y0 - z1 * z0 + w1 * w0;
    q68_69[1] = x1 * w0 + y1 * z0 - z1 * y0 + w1 * x0;
    q68_69[2] = -x1 * z0 + y1 * w0 + z1 * x0 + w1 * y0;
    q68_69[3] = x1 * y0 - y1 * x0 + z1 * w0 + w1 * z0;

}


        void setup_acc() {

            Serial.println("SETUP ACCELEROMENTER -------- BEGIN ");

            // Wire.begin(D6, D7);  
            Wire.begin(_pinA, _pinB);
            Wire.setClock(100000); // 100kHz
            Serial.begin(115200);
  
  // initialize device
  Serial.print("{\"key\": \"/log\", \"value\": \"Initializing device 0x68...\", \"level\": \"DEBUG\"}\n");
  _mpu68.initialize();
  error_code = _mpu68.dmpInitialize();
  
  // 1 = initial memory load failed
  // 2 = DMP configuration updates failed
  // (if it's going to break, usually the code will be 1)
  if (error_code == 1U) {
    Serial.print("{\"key\": \"/log\", \"value\": \"device 0x68 initialization failed: initial memory load failed.\", \"level\": \"ERROR\"}\n");
    while (1) {}
  }
  if (error_code == 2U) {
    Serial.print("{\"key\": \"/log\", \"value\": \"device 0x68 initialization failed: DMP configuration updates failed.\", \"level\": \"ERROR\"}\n");
    while (1) {}
  }
  
  Serial.print("{\"key\": \"/log\", \"value\": \"Initializing device 0x69...\", \"level\": \"DEBUG\"}\n");
  _mpu69.initialize();
  error_code = _mpu69.dmpInitialize();

  // 1 = initial memory load failed
  // 2 = DMP configuration updates failed
  // (if it's going to break, usually the code will be 1)
  if (error_code == 1U) {
    Serial.print("{\"key\": \"/log\", \"value\": \"device 0x68 initialization failed: initial memory load failed.\", \"level\": \"ERROR\"}\n");
    while (1) {}
  }
  if (error_code == 2U) {
    Serial.print("{\"key\": \"/log\", \"value\": \"device 0x68 initialization failed: DMP configuration updates failed.\", \"level\": \"ERROR\"}\n");
    while (1) {}
  }

  // verify connection
  if (!_mpu68.testConnection()) {
    Serial.print("{\"key\": \"/log\", \"value\": \"device 0x68 connection failed.\", \"level\": \"ERROR\"}\n"); 
  }
  if (!_mpu69.testConnection()) {
    Serial.print("{\"key\": \"/log\", \"value\": \"device 0x69 connection failed.\", \"level\": \"ERROR\"}\n");
  }

  // supply your own gyro offsets here, scaled for min sensitivity
  _mpu68.setXGyroOffset(0);
  _mpu68.setYGyroOffset(0);
  _mpu68.setZGyroOffset(0);
  _mpu68.setXAccelOffset(0);
  _mpu68.setYAccelOffset(0);
  _mpu68.setZAccelOffset(0);
  
  _mpu69.setXGyroOffset(0);
  _mpu69.setYGyroOffset(0);
  _mpu69.setZGyroOffset(0);
  _mpu69.setXAccelOffset(0);
  _mpu69.setYAccelOffset(0);
  _mpu69.setZAccelOffset(0);

  
  // Calibration Time: generate offsets and calibrate our MPU6050
  _mpu68.CalibrateAccel(6);
  _mpu68.CalibrateGyro(6);
  
  _mpu69.CalibrateAccel(6);
  _mpu69.CalibrateGyro(6);

  // calibration procedure will dump garbage on serial, we use a newline to fence it
  Serial.print("\n");
  
  // turn on the DMP, now that it's ready
  Serial.print("{\"key\": \"/log\", \"value\": \"Enabling DMP...\", \"level\": \"DEBUG\"}\n");
  _mpu68.setDMPEnabled(true);
  _mpu69.setDMPEnabled(true);
  Serial.print("{\"key\": \"/log\", \"value\": \"Device ready.\", \"level\": \"INFO\"}\n");
            
            
//gsr
            pinMode(_pinC, INPUT);
  for (int i=0; i<NUM; i++){
    arr[i]= analogRead(_pinC);
    media += arr[i];
  }
  media = media/NUM;
            
                //_espValues[0].setConfig(true, baseIP); //roll0
                _espValues[1].setConfig(true, baseIP); //pitch0
                //_espValues[2].setConfig(true, baseIP); //yaw0
                //_espValues[3].setConfig(true, baseIP); //roll1
                //_espValues[4].setConfig(true, baseIP); //pitch1
                _espValues[5].setConfig(true, baseIP); //yaw1
                //_espValues[6].setConfig(true, baseIP); //gsr
            Serial.println("SETUP TWO ACCELEROMETER AND GSR -------- COMPLETE\n");


            
        }

        void setup_sensor() {
            setup_acc();
        }


        void loop_acc() {

           uint8_t fifo_buffer68[64]; // FIFO storage buffer
           
  if (!_mpu68.dmpGetCurrentFIFOPacket(fifo_buffer68)) {
    //return;
  }

  uint8_t fifo_buffer69[64]; // FIFO storage buffer
  if (!_mpu69.dmpGetCurrentFIFOPacket(fifo_buffer69)) {
    //return;
  }
  

 //_mpu68IntStatus = _mpu68.getIntStatus();
 //_mpu69IntStatus = _mpu69.getIntStatus();
                
   //          _fifoCount68 = _mpu68.getFIFOCount();
   //          _fifoCount69 = _mpu69.getFIFOCount();

            
     //        if ((_mpu68IntStatus & 0x10) || (_mpu69IntStatus & 0x10)|| _fifoCount == 1024){ 
       //          _mpu68.resetFIFO();
         //       _mpu69.resetFIFO();
//             }

//             else if (_mpuIntStatus & 0x02) {
//                 while (_fifoCount < _packetSize) _fifoCount = _mpu.getFIFOCount();
                
//                 _mpu.getFIFOBytes(_fifoBuffer, _packetSize);
                
//                 _fifoCount -= _packetSize;
  
  // orientation/motion vars
  //Quaternion q68;           // [w, x, y, z]         quaternion container
  //Quaternion q69;           // [w, x, y, z]         quaternion container

  _mpu68.dmpGetQuaternion(&_q68, fifo_buffer68);
  _mpu69.dmpGetQuaternion(&_q69, fifo_buffer69);
  

  inverseMultiplyQuaternion(_q68, _q69, q68_69);
  
  //Serial.print("{\"key\": \"/joint/0\", \"value\": [");
  //Serial.print(q68.w);Serial.print(", ");
  //Serial.print(q68.x);Serial.print(", ");
  //Serial.print(q68.y);Serial.print(", ");
  //Serial.print(q68.z);
  //Serial.print("]}\n");
  
  //Serial.print("{\"key\": \"/joint/1\", \"value\": [");
  //Serial.print(q69.w);Serial.print(", ");
  //Serial.print(q69.x);Serial.print(", ");
  //Serial.print(q69.y);Serial.print(", ");
  //Serial.print(q69.z);
  //Serial.print("]}\n");

  //Serial.print("{difference: [");
  //Serial.print(q69.w- q68.w);Serial.print(", ");
  //Serial.print(q69.x-q68.x);Serial.print(", ");
  //Serial.print(q69.y-q68.y);Serial.print(", ");
  //Serial.print(q69.z-q68.z);
  //Serial.print("]}\n");

  /*
  //inverse multiply
  Serial.print(q68_69[0]);Serial.print(", ");
  Serial.print(q68_69[1]);Serial.print(", ");
  Serial.print(q68_69[2]);Serial.print(", ");
  Serial.print(q68_69[3]);Serial.println(", ");
  */

  data[2] = atan2(2*q68_69[1]*q68_69[2] - 2*q68_69[0]*q68_69[3], 2*q68_69[0]*q68_69[0] + 2*q68_69[1]*q68_69[1] - 1)*180/PI;
  data[1] = -asin(2*q68_69[1]*q68_69[3] + 2*q68_69[0]*q68_69[2])*180/PI;
  data[0] = atan2(2*q68_69[2]*q68_69[3] - 2*q68_69[0]*q68_69[1], 2*q68_69[0]*q68_69[0] + 2*q68_69[3]*q68_69[3] - 1)*180/PI;

    /*Serial.print(data[0]);Serial.print(", ");
  Serial.print(data[1]);Serial.print(", ");
  Serial.print(data[2]);Serial.println(", ");*/
  
    
  //delay(200);
            // euler angles roll pitch yaw
            // roll (x-axis rotation)
                double sinr_cosp = 2 * (_q68.w * _q68.x + _q68.y * _q68.z);
                double cosr_cosp = 1 - 2 * (_q68.x * _q68.x + _q68.y * _q68.y);
                _angles.roll0 = std::atan2(sinr_cosp, cosr_cosp);
                if((_angles.roll0 < 0.5 && _angles.roll0>0) || (_angles.roll0 > -0.5 && _angles.roll0<0) ){
                    _angles.roll0=0;
                }
                

                // pitch (y-axis rotation)
                double sinp = 2 * (_q68.w * _q68.y - _q68.z * _q68.x);
                if (std::abs(sinp) >= 1){
                    _angles.pitch0 = std::copysign(M_PI / 2, sinp); // use 90 degrees if out of range
                    }
                else
                    _angles.pitch0 = std::asin(sinp);
                if((_angles.pitch0 < 0.5 && _angles.pitch0>0) || (_angles.pitch0 > -0.5 && _angles.pitch0<0) ){
                    _angles.pitch0=0;
                }

                // yaw (z-axis rotation)
                
                double siny_cosp = 2 * (_q68.w * _q68.z + _q68.x * _q68.y);
                double cosy_cosp = 1 - 2 * (_q68.y * _q68.y + _q68.z * _q68.z);
                _angles.yaw0 = std::atan2(siny_cosp, cosy_cosp);
                if((_angles.yaw0 < 0.5 && _angles.yaw0>0) || (_angles.yaw0 > -0.5 && _angles.yaw0<0) ){
                    _angles.yaw0=0;
                }

                /*_angles.roll=roll;
                _angles.pitch=pitch;
                _angles.yaw=yaw;*/
                
                _angles.roll0 = atan2(2*_q68.y*_q68.z - 2*_q68.w*_q68.x, 2*_q68.w*_q68.w + 2*_q68.z*_q68.z - 1)*180/PI;
                _angles.pitch0 = -asin(2*_q68.x*_q68.z + 2*_q68.w*_q68.y)*180/PI;;
                _angles.yaw0 = atan2(2*_q68.x*_q68.y - 2*_q68.w*_q68.z, 2*_q68.w*_q68.w + 2*_q68.x*_q68.x - 1)*180/PI;
                _angles.roll1 = data[0];
                _angles.pitch1 = data[1];
                _angles.yaw1 = data[2];

                
                if((_angles.roll0 <20 && _angles.roll0 > -20) || _angles.roll0 <-160 || _angles.roll0> 160){
                    _angles.roll0=0;
                }
                if((_angles.roll1 <30 && _angles.roll1 > -30) || _angles.roll1 <-150 || _angles.roll1> 150){
                    _angles.roll1=0;
                }

                if((_angles.pitch0 <20 && _angles.pitch0 > -20) || _angles.pitch0 <-160 || _angles.pitch0> 160){
                    _angles.pitch0=0;
                }
                if((_angles.pitch1 <30 && _angles.pitch1 > -30) || _angles.pitch1 <-150 || _angles.pitch1> 150){
                    _angles.pitch1=0;
                }

                if((_angles.yaw0 <20 && _angles.yaw0 > -20) || _angles.yaw0 <-160 || _angles.yaw0> 160){
                    _angles.yaw0=0;
                }
                if((_angles.yaw1 <30 && _angles.yaw1 > -30) || _angles.yaw1 <-150 || _angles.yaw1> 150){
                    _angles.yaw1=0;
                }


         //GSR       
                
  a = arr[ind];
  arr[ind] = analogRead(_pinC);
  media = (media*NUM - a + arr[ind])/NUM;
  ind += 1;
  if (ind >= NUM){
    ind=0;
  }
  Serial.println(media);
  /*int value= map(media, 150, 1023, 255, 0);
  if(value>255){
    value=255;
  }
  if(value<0){
    value=0;
  }
  Serial.print(" ");
  Serial.println(value);*/
  //delay(10);
                
                

                _espValues[0].onNewValueReceived(_angles.roll0);
                _espValues[1].onNewValueReceived(_angles.pitch0);
                _espValues[2].onNewValueReceived(_angles.yaw0);
                _espValues[3].onNewValueReceived(_angles.roll1);
                _espValues[4].onNewValueReceived(_angles.pitch1);
                _espValues[5].onNewValueReceived(_angles.yaw1);
                _espValues[6].onNewValueReceived(media);
                

                /*
                _espValues[0].setConfig(true, baseIP);
                _espValues[1].setConfig(true, baseIP);
                _espValues[2].setConfig(true, baseIP);*/
               


                
                Serial.print(_angles.roll0);
                Serial.print(" - ");
                Serial.print(_angles.roll1);
                Serial.print(" - ");
                Serial.println(_angles.yaw0);
                Serial.println();
                delay(200);
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
