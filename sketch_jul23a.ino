char receivedChar;
bool newData = false;
bool isOn = false;

void setup() {
  // put your setup code here, to run once:
  Serial.begin(9600);
  Serial.setTimeout(50);
  pinMode(8, OUTPUT);
}

void loop() {
  recvOneChar();
}

void recvOneChar() {
  if (Serial.available() > 0) {
    
    receivedChar = Serial.read();

    Serial.write(receivedChar);
    
    if (receivedChar == 't') {
      digitalWrite(8, HIGH);
    }
    else if (receivedChar == 'f') {
      digitalWrite(8, LOW);
    }
  }
}