
#ifndef Included_LedMatrix_H

#define Included_LedMatrix_H

#include <Arduino.h>
#include <Adafruit_GFX.h>
#include <Adafruit_NeoMatrix.h>
#include <Adafruit_NeoPixel.h>
#include <LedMatrix_constants.h>


#define DEFAULT_MIN_BLINK_ELAPSED_MILLIS 1000
#define DEFAULT_MAX_BLINK_ELAPSED_MILLIS 5900
#define DEFAULT_BLINK_TIME 300


class LedMatrix
{
    private:
                
        uint8_t _pin;
        uint8_t _numPixels;  // num pixels of each axis. Must be the same for both axes and EVEN.
        Adafruit_NeoMatrix _matrix;
        uint64_t _brightness;
        uint64_t _eye_timeout_min;  // the MIN amount of time from a blink and another
        uint64_t _eye_timeout_max;  // the MAX amount of time from a blink and another. Timeout is always computed randomly in this interval

        uint8_t _pupil_center_range;  // the position of the pupuil is (0, 0) in the center. This is the max exctursion in both directions, 
                                      // and it's num_pixels/2 - 1
        uint8_t _start_idx = 0;
        uint8_t _end_idx;

        unsigned long _blinkStepDuration;

        bool dirty = true;

        unsigned int halfPixels;

        uint16_t _curXPupil = 0;
        uint16_t _curYPupil = 0;
        RGB _curColor;


//define constants 
int DELAY_EYE = 30; // in ms


    public:

        uint16_t currentEyePosX = 0;
        uint16_t currentEyePosY = 0;

        LedMatrix(uint8_t pin, int numPixels, uint64_t brightness, uint64_t eye_timeout_min, uint64_t eye_timeout_max) : _matrix(numPixels, numPixels, pin,
            NEO_MATRIX_TOP + NEO_MATRIX_RIGHT +
            NEO_MATRIX_ROWS + NEO_MATRIX_ZIGZAG,
            NEO_GRB + NEO_KHZ800) {
            _pin = pin;
            _numPixels = numPixels;
            _brightness = brightness;
            _eye_timeout_min = eye_timeout_min;
            _eye_timeout_max = eye_timeout_max;

            _pupil_center_range = (_numPixels / 2 ) - 1;
            _end_idx = _numPixels - 1;

            halfPixels = _numPixels / 2;
            float intermediate = DEFAULT_BLINK_TIME / (numPixels - 1.);
            _blinkStepDuration = intermediate;  // to convert from FLOAT
        }

        void setup() {
            //we initialize the matrix and configure its pixel brightness, text color and text wrapping options
            _matrix.begin();
            _matrix.setBrightness(_brightness);
        }

        void drawNet() {
            drawEye(neutral1, red1);
        }
        
        void drawOff() {
            drawEye(turned_off, vanilla);
        }

        // Check the pixel one after another and fill them if necessary
        void drawEye(bool eye[8][8], struct RGB color) {
            for(uint16_t row=0; row < 8; row++) {
                for(uint16_t column=0; column < 8; column++) {
                if (eye[row][column] == 1) { 
                    _matrix.drawPixel(column, row, _matrix.Color(color.r, color.g, color.b));
                }
                else
                {
                    _matrix.drawPixel(column, row, _matrix.Color(0, 0, 0));
                }
                }
            }
            _matrix.show();
        }

        bool isBorder(uint16_t x, uint16_t y) {
            return 
                ((x == _start_idx || x == _end_idx) && (y == _start_idx || y == _start_idx + 1 || y == _end_idx - 1 || y == _end_idx))
                || 
                ((x == _start_idx + 1 || x == _end_idx - 1) && (y == _start_idx || y == _end_idx));
        }

        bool isPupil(uint16_t x, uint16_t y, uint16_t xPupil, uint16_t yPupil) {
            uint16_t a = xPupil + _pupil_center_range;
            uint16_t b = yPupil + _pupil_center_range;

            return ((x == a || x == a + 1) && (y == b || y == b + 1));
        }

        void ddTrawDelay(uint16_t xPupil, uint16_t yPupil, struct RGB color, int delay_t) {
            draw(xPupil, yPupil, color);
            delay(delay_t);
        }

        void draw(uint16_t xPupil, uint16_t yPupil, struct RGB color) {
            // draws the ENTIRE EYE, given the position of the center of the pupil.
            // for each PIXEL POSITION, we must decide to draw either an ON pixel (eye) 
            // or an OFF pixel (border or pupil)
            //
            // for each axis, pupil position is in [-_pupil_center_range, _pupil_center_range]

            _curXPupil = xPupil;
            _curYPupil = yPupil;
            _curColor = color;

            for(uint16_t row=0; row < 8; row++) {
                if (isBlinkRow(row)) {
                    for(uint16_t column=0; column < 8; column++) {
                        _matrix.drawPixel(column, row, _matrix.Color(0, 0, 0));
                    }
                }
                else {
                    for(uint16_t column=0; column < 8; column++) {
                        if (isBorder(column, row) || isPupil(column, row, xPupil, yPupil)) { 
                            _matrix.drawPixel(column, row, _matrix.Color(0, 0, 0));
                        }
                        else
                        {
                            _matrix.drawPixel(column, row, _matrix.Color(color.r, color.g, color.b));
                        }
                    }
                }
            }
            _matrix.show();
        }

        void draw() {
            draw(_curXPupil, _curYPupil, _curColor);
        }

        void drawDelay(bool eye[8][8], struct RGB color, int delayTime) {
            drawEye(eye, color);
            delay(delayTime);
        }

        void testPupil() {

            for (size_t row = 0; row < 7; row++)
            {
                int rr = row - 3;
                for (size_t col = 0; col < 7; col++)
                {
                int cc = col - 3;

                ddTrawDelay(cc, rr, vanilla, 35);
                }
            }
            
            // delay(500);
        }


        void onPosXRcv(int newPosX) {
            currentEyePosX = newPosX;
            dirty = true;
        }

        void onPosYRcv(int newPosY) {
            currentEyePosY = newPosY;
            dirty = true;
        }

        void loop() {
            if (dirty) {
                draw(currentEyePosX, currentEyePosY, vanilla);
                dirty = false;
            }
        }

        // --- BLINK
        unsigned long blinkTimer;
        unsigned int blinkStep;
        unsigned long deltaTime;
        unsigned long blinkStateStartTime;
        bool isBlink;
        unsigned long blinkPauseDuration;
        unsigned long lastStepChangedTime;

        bool isBlinkRow(uint16_t row) {
            // from the BLINK STEP, check if the ROW is OFF. 
            // if it is, it's all gonna be off.
            // there are two extreme cases to check from the start:

            // Serial.print("ROW: ");
            // Serial.print(row);
            // Serial.print(" - BLINK STEP: ");
            // Serial.print(blinkStep);

            // case when blink is OFF
            if (blinkStep == 0){
                // Serial.println(" - STEP == 0; returning: FALSE");
                return false;
            }

            // case when all rows are off
            if (blinkStep >= halfPixels)
            {
                // Serial.println(" - > half pixels; returning: TRUE");
                return true;
            }

            // intermediate cases. 
            // the step is in [1, _numPixels/2 - 1] range
            // if numPixels is N and step is M, ALL ROWS from 0 to M-1, and from N-M+1 to N are OFF
            if (row < blinkStep || row >= _numPixels - blinkStep) {
                // Serial.println(" - returning: TRUE");
                return true;
            }
            else {
                // Serial.println(" - returning: FALSE");
                return false;
            }
        }

        // a BLINK is a sequence of "frames". In each frame a number of PAIRS or rows are turned off.
        // the duration of each frame depends on the total number of rows, and the total duration of the blink (forward and back motion)
        // a frame is described by the "blinkStep" int: 0 means NO BLINK (no rows are turned off because of blink).
        // 1: the outermost rows are off..
        // 2: the two outermost rows are off.. 
        // eccc...
        // A method called at EACH TIMESTEP compute the timer and decide if the "blinkStep" should be modified or not,
        // and updates the times
        // 
        // the number of ROWS of the matrix needs to be an EVEN number
        //
        // TWO possible STATES: "blink" and "notBlink"        
        // when a blink ENDS, a PAUSE DURATION is computed at random in an interval.
        // when a blink STARTS, the DURATION is the blink duration. 

        void blinkTick() {
            unsigned long now = millis();

            if (isBlink)
                isBlinkTick();
            else 
                isNotBlinkTick();
        }

        void isBlinkTick() {
            // check if elapsed time from the beginning of the not blink is higher than the PAUSE DURATION
            unsigned long now = millis();
            if (now - blinkStateStartTime >= DEFAULT_BLINK_TIME)
                stopBlink(now);
            else 
                computeBlinkStep(now);
        }

        int stepCounter;
        bool isIncreasing;

        void computeBlinkStep(unsigned long now) {
            // called during a IS BLINK state ticks
            // based on the amount of time elapsed from the BEGINNING of the BLINK, set the BLINK STEP
            // if there are 2N rows, and duration of the blink is D, then:
            // - there are N - 1 FRAMES. So the duration of each frame is D/(N - 1) = M.
            // every time the step is updated, the 'lastStepChangedTime' is updated to the current time. 
            // every time more than M has elasped from the 'lastStepChangedTime', change step. 
            // STEPS go from 1 to N, and BACK to 1!

            unsigned long delta = now - lastStepChangedTime;
            // Serial.print("DELTA: ");
            // Serial.print(delta);
            // Serial.print(" - _blinkStepDuration: ");
            // Serial.println(_blinkStepDuration);

            if (delta > _blinkStepDuration) {
                if (isIncreasing)
                    blinkStep ++;
                else 
                    blinkStep --;

                if (blinkStep >= halfPixels)
                    isIncreasing = false;

                lastStepChangedTime = now;
                // call the DRAW method if the STEP CHANGED
                draw();
            }
        }

        void isNotBlinkTick() {
            // check if elapsed time from the beginning of the not blink is higher than the PAUSE DURATION
            unsigned long now = millis();
            if (now - blinkStateStartTime >= blinkPauseDuration)
                startBlink(now);
        }

        void startBlink(unsigned long now) {
            // reset state FLAG
            isBlink = true;

            // reset START time
            blinkStateStartTime = now;

            // reset "last changed step time" to the current time
            lastStepChangedTime = now;

            // set blink step to the first one
            blinkStep = 1;
            // set the step counter to 1 as well. It's used to easily compute the STEP that also goes back by using "%"
            stepCounter = 1;
            isIncreasing = true;

            // call DRAW since the eye has changed by setting the FIRST blink
            draw();

            // Serial.print("--- START BLINK - ");
        }

        void stopBlink(unsigned long now) {
            // compute PAUSE duration
            blinkPauseDuration = random(DEFAULT_MIN_BLINK_ELAPSED_MILLIS, DEFAULT_MAX_BLINK_ELAPSED_MILLIS);

            // reset state FLAG
            isBlink = false;

            // reset START time
            blinkStateStartTime = now;

            // reset blink step
            blinkStep = 0;

            // draw becuase the eye changed
            draw();

            // Serial.print("--- STOP BLINK - pause duration: ");
            // Serial.println(blinkPauseDuration);
        }
};


#endif