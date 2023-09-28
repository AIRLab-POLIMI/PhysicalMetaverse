
#include <Arduino.h>


#ifdef ESP32
    #include <WiFi.h>
#else 
    #ifdef ESP8266
        #include <ESP8266WiFi.h>
        #include <string>
    #endif
#endif

#include <WiFiUdp.h>

using namespace std;


// ******** IP - DOF MAPPINGS
//
// ***TENDONS
// IP = 192.168.1.60 -> TENDON
// IP = 192.168.1.61 -> GLOVE ACCELEROMETER

// ----------------------------------------------------------------------------------------------DEFAULTS
// STATIC IP
/*
IPAddress default_gateway(192, 168, 1, 1);   // IP Address of your WiFi Router (Gateway)
IPAddress default_subnet(255, 255, 255, 0);  // Subnet mask
IPAddress default_primaryDNS(192, 168, 1, 1);  // DNS 1
IPAddress default_secondaryDNS(8, 8, 8, 8);  // DNS 2

// WIFI
#define WIFI_SSID "Physical Metaverse 2.4GHz"
#define WIFI_PSW "earthbound"
// UDP
#define MY_UDP_PORT 4210
#define IN_SIZE 255
#define OUT_SIZE 255
*/
//HONOR 9 LITE
/*IPAddress default_gateway(192, 168, 43, 1);   // IP Address of your WiFi Router (Gateway)
IPAddress default_subnet(255, 255, 255, 0);  // Subnet mask
IPAddress default_primaryDNS(192, 168, 43, 1);  // DNS 1
IPAddress default_secondaryDNS(8, 8, 8, 8);  // DNS 2
*/

//Physical Metaverse 2.4GHz     
IPAddress default_gateway(192, 168, 0, 1);   // IP Address of your WiFi Router (Gateway)
IPAddress default_subnet(255, 255, 255, 0);  // Subnet mask
IPAddress default_primaryDNS(192, 168, 0, 1);  // DNS 1
IPAddress default_secondaryDNS(8, 8, 8, 8);  // DNS 2

// WIFI
#define WIFI_SSID "Physical Metaverse 2.4GHz" //"Honor 9 Lite" 
#define WIFI_PSW "earthbound" //"ciao1234"
// UDP
#define MY_UDP_PORT 4210
#define IN_SIZE 255
#define OUT_SIZE 255
// COMMUNICATION
#define EMPTY_STRING ' '
#define DELIMITER ':'



// DEFAULT LED PINS
#ifdef ESP32
    const uint8_t default_ledPinOn = 27;    // digital GPIO27
    const uint8_t default_ledPinWiFi = 14;  // digital GPIO14
    const uint8_t default_ledPinFunction = 12; // digital GPIO12
#else 
    #ifdef ESP8266
        const uint8_t default_ledPinOn = 12;          // digital GPI12 - D6
        const uint8_t default_ledPinWiFi = 13;        // digital GPI13 - D7
        const uint8_t default_ledPinFunction = 15;    // digital GPI15 - D8   
    #endif
#endif




// ----------------------------------------------------------------------------------------------UTILS

class KeyValueMsg {
    
    public: 
        std::string key;
        std::string val;

        KeyValueMsg() {
            key = "";
            val = "";
        }

        KeyValueMsg(std::string in_key, std::string in_value) {
            key = in_key;
            val = in_value;
        }

        KeyValueMsg(char msg[]) {
            getKeyValue(msg);
        }

        /* convert CHAR ARRAY to KEY-VALUE pair if possible */
        void getKeyValue(char msg[]){
            
            std::string strMsg = std::string(msg);
            
            int delimiterIndex = strMsg.find(DELIMITER);
            
            std::string key = strMsg.substr (0, delimiterIndex);
            std::string value = strMsg.substr (delimiterIndex + 1);
        }

        /* check if current KEY is equal to the input STRING */
        bool isKeyEqualTo(std::string compareKey) {
            return key == compareKey;
        }

        /* convert current STRING VALUE to FLOAT and returns it */
        float floatValue() {
            try
            {
                // IF ONLY 'STOF' WORKED...
                // float floatVal = std::stof(value);
                return atof(val.c_str());
            }
            catch(const std::exception& e)
            {
                Serial.print("[KEY VALUE MSG][FLOATVALUE] - ERROR - could not convert VALUE: '");
                Serial.print(val.c_str());
                Serial.println("' to FLOAT - returning 0.");

                return 0;
            }
        }
};



// ----------------------------------------------------------------------------------------------MAIN CLASS


class EspUdp {

    private:

        KeyValueMsg _keyValueMsg;

    public: 

        // STATIC IP
        IPAddress m_staticIP; 
        IPAddress m_gateway;
        IPAddress m_subnet;
        IPAddress m_primaryDNS;
        IPAddress m_secondaryDNS;

        // WIFI
        char * m_wifi_ssid;  // should contain CHAR ARRAY
        char * m_wifi_psw;  // should contain CHAR ARRAY

        // UDP
        WiFiUDP UDP;
        char in_packet[IN_SIZE];
        char out_packet[OUT_SIZE];
        IPAddress m_defaultDestinationIP;  // should contain CHAR ARRAY
        int m_defaultDestinationPort;
        int m_in_size;
        int m_out_size;

        // LEDS
        uint8_t m_ledPinOn;    // LED ON when ESP ON
        uint8_t m_ledPinWiFi;  // LED ON when ESP connected to WIFI 
        uint8_t m_ledPinFunction; // additional LED for class-specific purposes (i.e. while button pressed)
        uint8_t leds[3];
        int numLeds;

        // VARS
        bool received;


        EspUdp(
            IPAddress staticIP, 
            IPAddress defaultDestinationIP, 

            int defaultDestinationPort = MY_UDP_PORT,  // the default is that all ESPs use the same port!

            uint8_t ledPinOn = default_ledPinOn,    // LED ON when ESP ON
            uint8_t ledPinWiFi = default_ledPinWiFi,  // LED ON when ESP connected to WIFI 
            uint8_t ledPinFunction = default_ledPinFunction, // additional LED for class-specific purposes (i.e. while button pressed)

            char wifi_ssid[] = WIFI_SSID,
            char wifi_psw[] = WIFI_PSW,

            int in_size = IN_SIZE,
            int out_size = OUT_SIZE,

            IPAddress gateway = default_gateway, 
            IPAddress subnet = default_subnet, 
            IPAddress primaryDNS = default_primaryDNS, 
            IPAddress secondaryDNS = default_secondaryDNS) {

            // STATIC IP
            m_staticIP  = staticIP; 
            m_gateway = gateway;
            m_subnet = subnet;
            m_primaryDNS = primaryDNS;
            m_secondaryDNS = secondaryDNS;

            // WIFI
            m_wifi_ssid = wifi_ssid;
            m_wifi_psw = wifi_psw;

            // UDP
            m_defaultDestinationIP = defaultDestinationIP;
            m_defaultDestinationPort = defaultDestinationPort;
            m_in_size = in_size;
            m_out_size = out_size;

            // LEDS
            m_ledPinOn = ledPinOn;    // LED ON when ESP ON
            m_ledPinWiFi = ledPinWiFi;  // LED ON when ESP connected to WIFI 
            m_ledPinFunction = ledPinFunction; // additional LED for class-specific purposes (i.e. while button pressed)

            leds[0] = m_ledPinOn;
            leds[1] = m_ledPinWiFi;
            leds[2] = m_ledPinFunction;

            numLeds = sizeof(leds)/sizeof(leds[0]);

            // VARS
            received = false;
        }

    // -----------------------------------------------------------------------LEDS 

        void setFunctionLed(bool high) {
            digitalWrite(m_ledPinFunction, high);
        }

        void blink_function_led(int delay_t, int num_times) {
            blink_led(m_ledPinFunction, delay_t, num_times);
        }

        void blink_led(uint8_t led_pin, int delay_t, int num_times = 1) {
        
            for (int i = 0; i < num_times; i++) {
                digitalWrite(led_pin, HIGH);
                delay(delay_t);
                digitalWrite(led_pin, LOW);
                delay(delay_t);
            }
        }

        void blink_leds(int delay_t, int num_times = 1) {

            for (int i = 0; i < num_times; i++) {
            
                for (int j = 0; j < numLeds; j++)
                    digitalWrite(leds[j], HIGH);  // because LEDS is a POINTER to the array of led pins

                delay(delay_t);
            
                for (int j = 0; j < numLeds; j++)
                    digitalWrite(leds[j], LOW);
                
                delay(delay_t); 
            }
        }

        void setup_leds() {

            Serial.println("[SETUP LEDS] - start");

            for (int j = 0; j < numLeds; j++)
                pinMode(leds[j], OUTPUT);
            
            blink_leds(100, 6);

            digitalWrite(m_ledPinOn, HIGH);
            digitalWrite(m_ledPinWiFi, LOW);
            digitalWrite(m_ledPinFunction, LOW);

            Serial.println("[SETUP LEDS] - complete\n");
        }

    // -----------------------------------------------------------------------WIFI 

        void connect_to_wifi(){

            Serial.println("[CONNECT TO WIFI] - begin");

            // Prevent connecting to wifi based on previous configuration
            WiFi.disconnect();  

            // setup with STATIC IP
            bool wifi_configured = false;
            while (!wifi_configured)
            {   
                Serial.println("CONNECTING");
                if (!WiFi.config(m_staticIP, m_gateway, m_subnet, m_primaryDNS, m_secondaryDNS)) {
                    Serial.println("[CONNECT TO WIFI] - failed to configure STATIC IP");
                    // blink fast to signal failed STATIC IP setup
                    blink_led(m_ledPinWiFi, 50, 5);
                    delay(1000);
                } else {
                    wifi_configured = true;
                    Serial.println("[CONNECT TO WIFI] - configured STATIC IP");
                }
            }

            // set the ESP32 to be a WiFi-client
            WiFi.mode(WIFI_STA);
            WiFi.begin(WIFI_SSID, WIFI_PSW);

            // Attempting connection to WiFi
            Serial.println("Trying to connect ...");
            while (WiFi.status() != WL_CONNECTED) {
                
                // blink the WIFI led while awaiting for connection once (per loop)
                // NB the "blink_led" method contains the delay
                blink_led(m_ledPinWiFi, 250);
                
                Serial.print(" .. not yet connected - current wifi status/connected status: ");
                Serial.print(WiFi.status());
                Serial.println("/");
                Serial.println(WL_CONNECTED);
            }

            // turn WIFI led OFF.
            // so, if LED stays off after blinking, it's because the UDP connection crashed the ESP
            digitalWrite(m_ledPinWiFi, LOW);

            // notify being connected to WiFi;
            Serial.print("Connected to Local Network - ESP IP: ");
            Serial.println(WiFi.localIP());

            WiFi.setAutoReconnect(true);
            WiFi.persistent(true);

            // Begin listening to UDP port
            UDP.begin(MY_UDP_PORT);
            Serial.print("UDP on:");
            Serial.println(MY_UDP_PORT);

            // turn WIFI led ON: WIFI connection successful
            digitalWrite(m_ledPinWiFi, HIGH);
            Serial.println("[CONNECT TO WIFI] - complete\n");
        }

    // -----------------------------------------------------------------------UDP
        
        #ifdef ESP32
                void add_char_msg_to_packet(char msg[]) {
                    UDP.print(msg);
                }
        #else 
            #ifdef ESP8266
                void add_char_msg_to_packet(char msg[]) {
                    UDP.write(msg);
                }
            #endif
        #endif

        // generic method used as template for ALL the "write" methods overloads that use a defualt IP and PORT. 
        // this automatically discriminates the case in which you want to use the DEFAULT destination 
        // or the REMOTE (last sender) one.
        // And handles possible errors (e.g. no last sender is present to respond to)
        // template <typename T>
        // void defaultWrite(T msg, bool remote, void (*writeMethod)(T, IPAddress, int)) {
        //     try {
        //         IPAddress ip = remote ? UDP.remoteIP() : m_defaultDestinationIP;    
        //         int port = remote ? UDP.remotePort() : m_defaultDestinationPort;

        //         writeMethod(msg, ip, port);
        //     }
        //     catch(const std::exception& e)
        //     {
        //         Serial.print("[WRITE CHAR UDP] - ERROR: '");
        //         Serial.print(e.what());
        //         Serial.println("'");
        //     }
        // }


        IPAddress remoteIP() {
            return UDP.remoteIP();
        }

        int remotePort() {
            return UDP.remotePort();
        }

        void write_char_udp(char msg[], IPAddress ip, int port){
            UDP.beginPacket(ip, port);
            add_char_msg_to_packet(msg);  // 
//            Serial.print("Sending packet: ");
//            Serial.println(msg);
            UDP.endPacket();
        }

        void write_char_udp(char msg[], bool remote = false){
            try {
                IPAddress ip = remote ? UDP.remoteIP() : m_defaultDestinationIP;    
                int port = remote ? UDP.remotePort() : m_defaultDestinationPort;

                write_char_udp(msg, ip, port);
            }
            catch(const std::exception & e)
            {
                Serial.print("[WRITE CHAR UDP] - ERROR: '");
                Serial.print(e.what());
                Serial.println("'");
            }
        }

        void write_String_udp(String msg, IPAddress ip, int port){
            Serial.println("***********************");
            Serial.println(ip);
            Serial.println(port);
            Serial.println("***********************");
            UDP.beginPacket(ip, port);
            UDP.print(msg);
            UDP.endPacket();
        }

        void write_string_udp(std::string msg, IPAddress ip, int port) {
            UDP.beginPacket(ip, port);
            UDP.print(msg.c_str());
            UDP.endPacket();
        }

        void write_int_udp(int value) {
            write_int_udp(value, m_defaultDestinationIP, m_defaultDestinationPort);
        }

        void write_int_udp(int value, IPAddress ip, int port){
            itoa(value, out_packet, 10);
        
            write_char_udp(out_packet, ip, port);
        }

        /* send key-value pair via UDP; 
        in this overload: KEY is a string - VAL is a FLOAT */
        void write_key_value_pair(std::string key, float val, char * ip, int port) {
            
            char charVal[20];
            
            UDP.beginPacket(ip, port);

            UDP.print(key.c_str());
            UDP.write(DELIMITER);

            // convert FLOAT val to *CHAR
            // - 15 is min size (min number of characters) - 
            // - 10 is number of decimals to truncate to - 
            // - charVal is char[] butter to write in - 
            UDP.print(dtostrf(val, 15, 10, charVal));

            UDP.endPacket();
        }

        int read_udp_non_blocking(){
            
            int packetSize = UDP.parsePacket();

            received = false;
            int led = -1;
            
            if (packetSize) {
                Serial.print("Received packet! Size: ");
                Serial.println(packetSize); 
                int len = UDP.read(in_packet, 10);  // the value is written in the BUFFER specificed as the first argument ("in_packet" in our case)
                int in_msg = in_packet[0] | in_packet[1] << 8;
                if (len > 0)
                {
                    in_packet[len] = '\0';
                    if(in_msg == 256) led = 1;
                    else if(in_msg == 0) led = 0;
                    received = true;
                }
                Serial.print("Packet received: ");
                Serial.print(in_msg);
                Serial.print(" - with size: ");
                Serial.print(len);
                Serial.print(" - current size: ");
                Serial.println(sizeof(in_packet));
            }

            return led;
        }

        /* check if the current IN PACKET from UDP is the same as the 
        msg specified as INPUT to the method */
        bool udp_msg_equals_to(char compare_msg[]) {
            return strcmp(in_packet, compare_msg) == 0;
        }

        /* check if the two input CHAR[] are equal */
        bool char_msgs_are_equal(char msg1[], char msg2[]) {
            return strcmp(msg1, msg2) == 0;
        }

    // -----------------------------------------------------------------------KEYVALUEMSG WRAPPERS

        /* GETTERS */  
        std::string key() {

            try
            {
                return _keyValueMsg.key;
            }
            catch(const std::exception& e)
            {
                Serial.print("[ESP UDP][KEY] - ERROR: ");
                Serial.println(e.what());
                return "NO KEY.";
            }
        }

        std::string val() {
            try
            {
                return _keyValueMsg.val;
            }
            catch(const std::exception &e)
            {
                Serial.print("[ESP UDP][VAL] - ERROR: ");
                // Serial.println(e.what());
                return "NO VAL.";
            }
        }

        /* convert IN PACKET to KEY-VALUE pair if possible */
        void getKeyValue(){        
            try
            {
                _keyValueMsg.getKeyValue(in_packet);
            }
            catch(const std::exception& e)
            {
                Serial.print("[ESPUDP][GET KEY VALUE] - ERROR: ");
                Serial.println(e.what());
            }
        }

        /* check if current KEY is equal to the input STRING */
        bool isKeyEqualTo(std::string compareKey) {
            return _keyValueMsg.key == compareKey;
        }

        /* convert current STRING VALUE of KEYVALUEMSG to FLOAT and returns it
        if it is not possible, return 0 */
        float floatValue() {
            return _keyValueMsg.floatValue();
        }

    // -----------------------------------------------------------------------SETUP

        void setup() {

            Serial.println("[ESPUDP][SETUP]  ---------------------- START");

            setup_leds();

            connect_to_wifi();

            Serial.println("[ESPUDP][SETUP]  ---------------------- COMPLETE");
        }
};
