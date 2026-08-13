/*
 * Serial.cpp
 *
 *  Created on: Feb 8, 2016
 *      Author: d
 */

#include "Serial.h"
#include "FireflyUtils.h"

#define WIN_COM "COM9"
#define MAC_COM "/dev/cu.usbmodem27946701"
#define COM_BAUD 9600

Serial::Serial() {
	// TODO Auto-generated constructor stub
	#if defined _WIN32 || defined _WIN64
	#else
		serial_mac = 0;
	#endif
}

Serial::~Serial() {
	// TODO Auto-generated destructor stub
}

bool Serial::initComms() {
#if defined _WIN32 || defined _WIN64
	serial_pc = new SerialPC(WIN_COM);
#else
	log("Right before serial!");
	serial_mac = serialport_init(MAC_COM, COM_BAUD);
	log("Mac! Port " + to_string(serial_mac));
#endif
	return true;
}

bool Serial::available() {
#if defined _WIN32 || defined _WIN64
	return serial_pc->IsConnected();
#else
	return serial_mac > 0;
#endif
}

int Serial::write(const char* str,
		int size) {
#if defined _WIN32 || defined _WIN64
	return serial_pc->WriteData(str, size);
#else
	return serialport_write(serial_mac, str, size);
#endif
}

int Serial::read(char* str,
		int size) {
#if defined _WIN32 || defined _WIN64
	return serial_pc->ReadData(str, size);
#else
	while(serialport_read_until(serial_mac, str, '\0', size, 100000) != 0) {};
	return size;
#endif
}
