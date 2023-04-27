#include <MPU6050_tockn.h>
#include <Wire.h>

MPU6050 mpu6050(Wire);

long timer = 0;


void setup() {
  Serial.begin(9600);
  Wire.begin();
  mpu6050.begin();
}

void loop() {
  mpu6050.update();

  if(millis() - timer > 125){
    Serial.print("1");Serial.print(" ");
    float X = mpu6050.getAccX();Serial.print(X*X);Serial.print(" ");
    float Y = mpu6050.getAccY();Serial.print(Y*Y);Serial.print(" ");
    float Z = mpu6050.getAccZ();Serial.println(Z*Z);
    
    timer = millis();
  }
}
