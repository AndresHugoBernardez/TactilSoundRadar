//Serial Communication from Arduino
//
//
//  Find a tutorial for How to use Arduino IDE compilator into SimulIDE
//
//


#define L0 3  
#define L1 4
#define L2 5
#define R0 6
#define R1 7
#define R2 8


int LeftByte=0;
int RightByte=0;


void setup(){

pinMode(L0,INPUT);
pinMode(L1,INPUT);
pinMode(L2,INPUT);
pinMode(R0,INPUT);
pinMode(R1,INPUT);
pinMode(R2,INPUT);


Serial.begin(9600);

Serial.println("Headphone Sound Radar");
Serial.println("Andy's Softwares");
delay(22);

}

void loop(){
LeftByte=0;
LeftByte+=digitalRead(L0);
LeftByte+=2*digitalRead(L1);
LeftByte+=4*digitalRead(L2);

//Invert input
LeftByte=7-LeftByte;

RightByte=0;
RightByte+=digitalRead(R0);
RightByte+=2*digitalRead(R1);
RightByte+=4*digitalRead(R2);

//Invert input
RightByte=7-RightByte;

Serial.print("L");
Serial.print(LeftByte);
Serial.print("R");
Serial.print(RightByte);
Serial.println();
delay(22);
}

