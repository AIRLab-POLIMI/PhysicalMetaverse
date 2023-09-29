
#ifndef Included_Thermosensor_H

#define Included_Thermosensor_H

#include <Arduino.h>
#include <Wire.h>
#include <Adafruit_MLX90614.h>


class Thermosensor
{
    private:
        Adafruit_MLX90614 _mlx = Adafruit_MLX90614();

        float _errTemp = 0.7f;
        float _calibrationSum = 0;               // Sum computed during calibration
        unsigned int _calibrationCounter = 0;    // Samples considered in the mean
        float _meanTemp;
        float _deltaTempHand;  // the mean must be above this temperature to register a hand

        // #### TIME ####
        unsigned long _timer_calibration;

        // COMM
        String _ID;
        float _currentTemp;
        float _lastSentValue;
        float _tolerance;
        

    public:

        Thermosensor(String ID, float deltaTempHand, float tolerance) {
            _ID = ID;
            _deltaTempHand = deltaTempHand;
            _tolerance = tolerance;

            _currentTemp = 0;
            _lastSentValue = -1;
        }

        void setup(int calibrationTime) {
            _mlx.begin(); 

            _timer_calibration = millis();    
            // calibration
            while (millis() - _timer_calibration < calibrationTime){
                calibrateTemperature();        
            }
            _meanTemp = _calibrationSum / _calibrationCounter;
            #if defined(DEVMODE)
                Serial.print("Thermosensor SETUP begin. Mean temperature:"); 
                Serial.println(_meanTemp);
            #endif
        }

        // Compute the calibration data
        void calibrateTemperature() {
            // Acquire the temperature and update the sum
            _calibrationSum += _mlx.readObjectTempC();
            // Increment the counter
            _calibrationCounter++; 
        }

        void loop() {
            // check temperature and save the current value
            readTemperature();
        }

        boolean readTemperature() {
            float objectTemp = _mlx.readObjectTempC();
            // _currentTemp = objectTemp;

            #if defined(DEVMODE)
                Serial.print("Thermo State: Object temperature: ");
                Serial.println(objectTemp);
            #endif

            if(objectTemp >= _meanTemp + _deltaTempHand) {
                //There is a hand that is really near the sphere of the robot
                #if defined(DEVMODE)
                    Serial.print("     \--- > HAND DETECTED.");
                #endif 
                _currentTemp = 1;
                return true;
            }
            _currentTemp = 0;
            return false;    
        }

        String getKeyValueMsg(String delimiter) {
            String msg;
            
            if (abs(_lastSentValue - _currentTemp) > _tolerance) {
                msg = _ID + delimiter + String(_currentTemp);
                _lastSentValue = _currentTemp;
            }
            else 
                msg = "";

            return msg;
        }
};


#endif