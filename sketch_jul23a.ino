char receivedChar;
boolean newData = false;

void setup() {
  // put your setup code here, to run once:
  Serial.begin(9600);
  pinMode(13, OUTPUT);
  pinMode(8, OUTPUT);
}

void loop() {
  recvOneChar();
  power();
}

void recvOneChar() {
  if (Serial.available() > 0) {
    receivedChar = Serial.read();
    newData = true;
  }
}

void power() {
  if (newData == true) {
    if (receivedChar == 't') {
      digitalWrite(8, HIGH);
    }
    else {
      digitalWrite(8, LOW);
    }
    newData = false;
  }
}