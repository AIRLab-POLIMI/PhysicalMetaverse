

#ifndef Included_Triskarbase_H

#define Included_Triskarbase_H

#include <Arduino.h>

#define MAX_DC_MOTOR_SIGNAL 100 //255 
#define NMOTOR 3
#define DEFAULT_MIN_SPEED 3

#define FREQUENCY 16000

class TriskarBase
{
    protected:

        // ---gloabl speed targets
        // TRISKAR BASE
        float _forwardSpeedTarget;
        float _strafeSpeedTarget;
        float _angularSpeedTarget;

        float _wheelRadius;  // 3.5f //cm
        float _robotRadius;  // 12.5f  //cm
        float _m1_R;    // (-1.0f / wheel_radius)
        float _mL_R;    // (-robot_radius / wheel_radius)
        float _C60_R;   // (0.500000000f / wheel_radius)   // cos(60°) / R
        float _C30_R;   // (0.866025404f / wheel_radius)   // cos(30°) / R

        // PINS
        uint8_t _mRA;
        uint8_t _mRB;
        uint8_t _mLA;
        uint8_t _mLB;
        uint8_t _mBA;
        uint8_t _mBB;


        virtual void set_wheel_speed(float speed, int pinA, int pinB) = 0;


        void clamp_speeds_to_zero() {
            if (abs(_forwardSpeedTarget) < DEFAULT_MIN_SPEED)
                _forwardSpeedTarget = 0;
            if (abs(_strafeSpeedTarget) < DEFAULT_MIN_SPEED)
                _strafeSpeedTarget = 0;
            if (abs(_angularSpeedTarget) < DEFAULT_MIN_SPEED)
                _angularSpeedTarget = 0;
        }

        virtual void setRadius() = 0;

        void setProportions() {

            Serial.println(_robotRadius);
            Serial.println(_wheelRadius);

            _m1_R = (-1.0f / _wheelRadius);
            _mL_R = -_robotRadius / _wheelRadius;
            _C60_R = (0.500000000f / _wheelRadius);   // cos(60°) / R
            _C30_R = (0.866025404f / _wheelRadius);   // cos(30°) / R

            Serial.print(" - _m1_R: ");
            Serial.print(_m1_R);
            Serial.print(" - _mL_R: ");
            Serial.print(_mL_R);
            Serial.print(" - _C60_R: ");
            Serial.print(_C60_R);
            Serial.print(" - _C30_R: ");
            Serial.println(_C30_R);
        }

    public:

        void setup (uint8_t mRA, uint8_t mRB, uint8_t mLA, uint8_t mLB, uint8_t mBA, uint8_t mBB) {
            
            // set pins
            _mRA = mRA;
            _mRB = mRB;
            _mLA = mLA;
            _mLB = mLB;
            _mBA = mBA;
            _mBB = mBB;
            pinMode(_mRA, OUTPUT);
            pinMode(_mRB, OUTPUT);
            pinMode(_mLA, OUTPUT);
            pinMode(_mLB, OUTPUT);
            pinMode(_mBA, OUTPUT);
            pinMode(_mBB, OUTPUT);
            TCCR2B = TCCR2B & B11111000 | B0000001; // for PWM frequency of 31372.55 Hz
            TCCR1B = TCCR1B & B11111000 | B0000001;
            TCCR0B = TCCR0B & B11111000 | B0000001;

            // set proportions
            setRadius();
            setProportions();
        }

        void setForwardTarget(float forwardTarget) {
            _forwardSpeedTarget = forwardTarget;
        }
        void setStrafeTarget(float strafeTarget) {
            _strafeSpeedTarget = strafeTarget;
        }
        void setAngularTarget(float angularTarget) {
            _angularSpeedTarget = angularTarget;
        }

        void setSpeedTargets(float forwardTarget, float strafeTarget, float angularTarget) {
            _forwardSpeedTarget = forwardTarget;
            _strafeSpeedTarget = strafeTarget;
            _angularSpeedTarget = angularTarget;
        }

        float getForwardTarget() {
            return _forwardSpeedTarget;
        }

        float getStrafeTarget() {
            return _strafeSpeedTarget;
        }

        float getAngularTarget() {
            return _angularSpeedTarget;
        }

        void setWheelSpeeds() {
            // MULTIPLY FORWARD SPEED BY 550
            // MULTIPLY STRAFE SPEED BY 550
            // MULTIPLY angANGULAR SPEED BY 250
            
            /*const float dx12 = _C60_R * _strafeSpeedTarget * 500;
            const float dy12 = _C30_R * _forwardSpeedTarget * 500;
            const float dthz123 = _mL_R * _angularSpeedTarget * 40;*/
            const float dx12 = _C60_R * _strafeSpeedTarget * 10; //200
            const float dy12 = _C30_R * _forwardSpeedTarget * 15;
            const float dthz123 = _mL_R * _angularSpeedTarget * 2; //30

            const float speed_target_R = dx12 + dy12 + dthz123; //motore anteriore dx
            const float speed_target_L = dx12 - dy12 + dthz123; //motore anteriore sx
            const float speed_target_B = (_m1_R * _strafeSpeedTarget * 10) + dthz123; // motore posteriore

            Serial.print(" - _m1_R: ");
            Serial.print(_m1_R);
            Serial.print(" - _mL_R: ");
            Serial.print(_mL_R);
            Serial.print(" - _C60_R: ");
            Serial.print(_C60_R);
            Serial.print(" - _C30_R: ");
            Serial.print(_C30_R);
            Serial.print(" - dx12: ");
            Serial.print(dx12);
            Serial.print(" - dy12: ");
            Serial.print(dy12);
            Serial.print(" - dthz123: ");
            Serial.print(dthz123);
            Serial.print(" - _forwardSpeedTarget: ");
            Serial.print(_forwardSpeedTarget);
            Serial.print(" - _strafeSpeedTarget: ");
            Serial.print(_strafeSpeedTarget);
            Serial.print(" - _angularSpeedTarget: ");
            Serial.print(_angularSpeedTarget);
            Serial.print(" - speed_target_R: ");
            Serial.print(speed_target_R);
            Serial.print(" - speed_target_L: ");
            Serial.print(speed_target_L);
            Serial.print(" - speed_target_B: ");
            Serial.println(speed_target_B);


            clamp_speeds_to_zero();

            set_wheel_speed(speed_target_R, _mRA, _mRB);
            set_wheel_speed(speed_target_L, _mLA, _mLB);
            set_wheel_speed(speed_target_B, _mBA, _mBB);
        }
};


class TriskarBaseMid: public TriskarBase {
    
    private: 

        void set_wheel_speed(float speed, int pinA, int pinB){
            // Make sure the speed is within the limit.
            if (speed > MAX_DC_MOTOR_SIGNAL) {
                speed = MAX_DC_MOTOR_SIGNAL;
            } else if (speed < -MAX_DC_MOTOR_SIGNAL) {
                speed = -MAX_DC_MOTOR_SIGNAL;
            }
            
            // Set the speed and direction.
            if (speed >= 0) {
                analogWrite(pinA, speed);
                analogWrite(pinB, 0);
            } else {
                analogWrite(pinA, 0);
                analogWrite(pinB, -speed);
            }
        }

        void setRadius() {
            _robotRadius = 12.5f;
            _wheelRadius = 3.5f;
        }
};

class TriskarBaseBig: public TriskarBase {

    private: 

        void set_wheel_speed(float speed, int pinA, int pinB){
            // Make sure the speed is within the limit.
            if (speed > MAX_DC_MOTOR_SIGNAL) {
                speed = MAX_DC_MOTOR_SIGNAL;
            } else if (speed < -MAX_DC_MOTOR_SIGNAL) {
                speed = -MAX_DC_MOTOR_SIGNAL;
            }
            
            Serial.print("[set_wheel_speed] - speed: ");
            Serial.print(speed);
            Serial.print(" - pinA: ");
            Serial.print(pinA);
            Serial.print(" - pinB: ");
            Serial.println(pinB);
            
            // Set the speed and direction.
            if (speed >= 0) {
                analogWrite(pinA, speed);
                digitalWrite(pinB, true);
            } else {
                analogWrite(pinA, -speed);
                digitalWrite(pinB, false);
            }
        }

        void setRadius() {
            _robotRadius = 13.5f;
            _wheelRadius = 5.0f;
        }

};






#endif