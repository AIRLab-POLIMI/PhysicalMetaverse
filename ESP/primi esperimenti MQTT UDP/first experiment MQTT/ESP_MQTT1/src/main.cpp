/*********
  Rui Santos
  Complete project details at https://randomnerdtutorials.com  
*********/

#include <WiFi.h>
#include <PubSubClient.h>
#include <MPU6050_tockn.h>


// Replace the next variables with your SSID/Password combination
const char* ssid = "Galaxy A321FFC";
const char* password = "sfdh6363";

// Add your MQTT Broker IP address, example:
//const char* mqtt_server = "192.168.1.144";
const char* mqtt_server = "192.168.185.162";

WiFiClient espClient;
PubSubClient client(espClient);
char msg[50];
int value = 0;
const char* device_name = "ESP32";


void setup_wifi() {
  //delay(10);
  // We start by connecting to a WiFi network
  Serial.println();
  Serial.print("Connecting to ");
  Serial.println(ssid);

  WiFi.begin(ssid, password);

  int attempts =0;
  while (WiFi.status() != WL_CONNECTED) {
    delay(500);
    attempts++;
    if(attempts > 10){
      ESP.restart();
    }
    Serial.print(".");
  }

  Serial.println("");
  Serial.println("WiFi connected");
  Serial.println("IP address: ");
  Serial.println(WiFi.localIP());
}

void callback(char* topic, byte* message, unsigned int length) {
  Serial.print("Message arrived on topic: ");
  Serial.print(topic);
  Serial.print(". Message: ");
  String messageTemp;
  
  for (int i = 0; i < length; i++) {
    Serial.print((char)message[i]);
    messageTemp += (char)message[i];
  }
  Serial.println();

  // Feel free to add more if statements to control more GPIOs with MQTT

  // If a message is received on the topic esp32/output, you check if the message is either "on" or "off". 
  // Changes the output state according to the message
  if (String(topic) == "esp32/x") {
    Serial.print("Changing output to ");
    if(messageTemp == "message"){
      client.publish("esp32/x", "message received");
      Serial.println("ack");
    }
    
  }
}

void reconnect() {
  // Loop until we're reconnected
  while (!client.connected()) {
    Serial.print("Attempting MQTT connection...");
    // Attempt to connect
    if (client.connect(device_name)) {
      Serial.println("connected");
      // Subscribe
      client.subscribe("esp32/x");
    } else {
      Serial.print("failed, rc=");
      Serial.print(client.state());
      Serial.println(" try again in 5 seconds");
      // Wait 5 seconds before retrying
      delay(5000);
    }
  }
}

void setup() {
  Serial.begin(115200);
  // default settings
  // (you can also pass in a Wire library object like &Wire2)
  //status = bme.begin();  
  
  setup_wifi();
  client.setServer(mqtt_server, 1883);
  client.setCallback(callback);

}



void loop() {
  if (!client.connected()) {
    reconnect();
  }
  client.loop();

}