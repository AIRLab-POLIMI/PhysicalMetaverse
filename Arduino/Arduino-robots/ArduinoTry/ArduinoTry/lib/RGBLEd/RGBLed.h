
#ifndef Included_RGBLed_H

#define Included_RGBLed_H

#include <Arduino.h>


class RGBLed
{
    private:
        uint8_t pins[3];
        bool dirty = true;

    public:

        int currentVal = 0;

        RGBLed(uint8_t redPin, uint8_t greenPin, uint8_t bluePin) {
            pins[0] = redPin;
            pins[1] = greenPin;
            pins[2] = bluePin;
        }

        void ledOff() {
            // if all PIN values are LOW, 
            // the RGB led will be OFF
            // for (size_t i = 0; i < 3; i++)
            // {
            //     digitalWrite(pins[i], LOW);
            // }
            writeBrightness(0);
        }

        void ledOn() {
            // the RGB led will be ON (brightest white)
            writeColor(255, 255, 255);
        }

        void setup() {
            // set PIN MODEs
            for (size_t i = 0; i < 3; i++)
            {
                pinMode(pins[i], OUTPUT);
            }

            // turn led OFF
            ledOff();
        }

        void writeColor(int red, int green, int blue) {
            // for each color accept a value in [0, 255].
            // each color pin wants a value in [0, 255].

            analogWrite(pins[0], red);
            analogWrite(pins[1], green);
            analogWrite(pins[2], blue);
        }

        void writeBrightness(int brightness) {
            if (brightness > 255)
                brightness = 255;
            else if (brightness < 0)
                brightness = 0;

            writeColor(brightness, brightness, brightness);
        }

        void onValueRcv(int newVal) {
            // input value is in [0, 255].
            currentVal = newVal;
            dirty = true;
        }

        void loop() {
            if (dirty) {
                // Serial.println(currentVal);
                writeBrightness(currentVal);
                dirty = false;
            }
        }
};


#endif