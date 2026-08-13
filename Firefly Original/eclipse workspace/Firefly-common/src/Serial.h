/*
 * Serial.h
 *
 *  Created on: Feb 8, 2016
 *      Author: d
 */

#ifndef SRC_SERIAL_H_
#define SRC_SERIAL_H_

#if defined _WIN32 || defined _WIN64
	#include "Serial-PC.h"
#else
	#include "arduino-serial-lib.h"
#endif


class Serial {
private:
#if defined _WIN32 || defined _WIN64
	SerialPC *serial_pc;
#else
	int serial_mac;
#endif

public:
	Serial();
	virtual ~Serial();

	bool initComms();
	bool available();
	int write(const char* str, int size);
	int read(char* str, int size);

};

#endif /* SRC_SERIAL_H_ */
