// --- Pin Definitions ---

// IR Sensors
const int LEFT_IR_PIN = 2;
const int RIGHT_IR_PIN = 3;
// Left Motor (L298N IN1, IN2)
const int LEFT_MOTOR_IN1 = 5;
const int LEFT_MOTOR_IN2 = 6;
// Right Motor (L298N IN3, IN4)
const int RIGHT_MOTOR_IN3 = 9;
const int RIGHT_MOTOR_IN4 = 10;
// Note on Sensor Logic:
// Most digital IR line sensors output LOW on reflective white and HIGH on black line
// (or vice-versa depending on the module). We define them here for clarity:
#define ON_LINE HIGH
#define OFF_LINE LOW
void setup()
{
  // Initialize IR sensor pins as digital inputs
  pinMode(LEFT_IR_PIN, INPUT);
  pinMode(RIGHT_IR_PIN, INPUT);
  // Initialize motor control pins as digital outputs
  pinMode(LEFT_MOTOR_IN1, OUTPUT);
  pinMode(LEFT_MOTOR_IN2, OUTPUT);
  pinMode(RIGHT_MOTOR_IN3, OUTPUT);
  pinMode(RIGHT_MOTOR_IN4, OUTPUT);
  // Ensure motors start in a stopped state
  stopMotors();
}
void loop()
{
  // Read current states of both IR sensors
  int leftState = digitalRead(LEFT_IR_PIN);
  int rightState = digitalRead(RIGHT_IR_PIN);
  // Decision-making logic for differential steering
  if (leftState == ON_LINE && rightState == ON_LINE)
  {
    // Both sensors detect black line: Move straight forward
    moveForward();
  }
  else if (leftState == OFF_LINE && rightState == ON_LINE)
  {
    // Robot drifted left; right sensor is on line: Turn right to correct
    turnRight();
  }
  else if (leftState == ON_LINE && rightState == OFF_LINE)
  {
    // Robot drifted right; left sensor is on line: Turn left to correct
    turnLeft();
  }
  else
  {
    // Both sensors off line (line lost or white surface): Stop or forward
    stopMotors();
  }
}
// --- Motor Control Helper Functions ---
void moveForward()
{
  // Left motor forward
  digitalWrite(LEFT_MOTOR_IN1, HIGH);
  digitalWrite(LEFT_MOTOR_IN2, LOW);
  // Right motor forward
  digitalWrite(RIGHT_MOTOR_IN3, HIGH);
  digitalWrite(RIGHT_MOTOR_IN4, LOW);
}
void turnLeft()
{
  // Pivot left: Stop left motor, drive right motor forward
  digitalWrite(LEFT_MOTOR_IN1, LOW);
  digitalWrite(LEFT_MOTOR_IN2, LOW);
  digitalWrite(RIGHT_MOTOR_IN3, HIGH);
  digitalWrite(RIGHT_MOTOR_IN4, LOW);
}
void turnRight()
{
  // Pivot right: Drive left motor forward, stop right motor
  digitalWrite(LEFT_MOTOR_IN1, HIGH);
  digitalWrite(LEFT_MOTOR_IN2, LOW);
  digitalWrite(RIGHT_MOTOR_IN3, LOW);
  digitalWrite(RIGHT_MOTOR_IN4, LOW);
}
void stopMotors()
{

  // Halt both motors
  digitalWrite(LEFT_MOTOR_IN1, LOW);
  digitalWrite(LEFT_MOTOR_IN2, LOW);
  digitalWrite(RIGHT_MOTOR_IN3, LOW);
  digitalWrite(RIGHT_MOTOR_IN4, LOW);
}