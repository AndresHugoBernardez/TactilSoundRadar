


#define Fw A0
#define Bw A1


#define L1 2
#define L2 3
#define L3 4

#define L4 5
#define L5 6
#define L6 7

#define L7 8
#define L8 9
#define L9 10

#define offsetPin A5




void setup(){

	pinMode(Fw,INPUT);
	pinMode(Bw,INPUT);
 pinMode(offsetPin,INPUT);

pinMode(L1, OUTPUT);
pinMode(L2, OUTPUT);
pinMode(L3, OUTPUT);
pinMode(L4, OUTPUT);
pinMode(L5, OUTPUT);
pinMode(L6, OUTPUT);
pinMode(L7, OUTPUT);
pinMode(L8, OUTPUT);
pinMode(L9, OUTPUT);
	
		
		
	Serial.begin(9600);
	Serial.println("Hear Position");
	delay(30);


	



}


int forward=1023;
int backward=1023;

int currentForward=1023;
int currentBackward=1023;

int LightF;
int newF;
int LightB;
int newB;

int humbral=0;

int humbral1=800;
int humbral2=600;
int humbral3=400;
int humbral4=200;


int offset=0;
int i=7;



void loop(){
    
 
 if(i>13){offset=analogRead(offsetPin);
        i=0;
        
        humbral=(1023-offset)/5;
        humbral1=1023-humbral;
        humbral2=1023-2*humbral;
        humbral3=1023-3*humbral;
        humbral4=1023-4*humbral;
        
        
        
        
    }
    else i++;
    
    
 
 currentBackward=	analogRead(Bw)+1;
	currentForward=		analogRead(Fw)+1;
 
 
backward = (currentBackward + 7 * backward) >> 3;
forward  = (currentForward  + 7 * forward) >> 3;



	Serial.print("Points(F:");
	Serial.print(forward);
	Serial.print(",B:");
	Serial.print(backward);
	Serial.println(")");


	
    
    
 
	
    
 if(backward>humbral1 && forward>humbral1) {
        newB=L5;
        newF=L5;
        }
 else{
        
        
        if(backward>humbral2)newB=L6;
            
        
 else{
    
     if( backward>humbral3)newB=L7;
 else{
        
                
                           if( backward>humbral4)newB=L8;
 else{
                    
        newB=L9;
 
    
    }    
               
    }    
    }    
    
    if( forward >humbral2)newF=L4;
 else{
        if(forward>humbral3)newF=L3;
 else{
                
                
            if( forward>humbral4)newF=L2;
 else{
        newF=L1;
    
    }
    
    }    
    }    
    
 }    
    
    
    
 if(newF!=LightF){
        digitalWrite(LightF,LOW);
        LightF=newF;
        digitalWrite(LightF,HIGH);
    }
 if(newB!=LightB){
        digitalWrite(LightB,LOW);
        LightB=newB;
        digitalWrite(LightB,HIGH);
    }
			
	delay(10);
	
}