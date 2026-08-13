/*
 * ArbitraryMap.cpp
 *
 *  Created on: Aug 13, 2016
 *      Author: d
 */

#include <animation/base/ArbitraryMap.h>

ArbitraryMap::ArbitraryMap() {
	// TODO Auto-generated constructor stub

}

ArbitraryMap::~ArbitraryMap() {
	// TODO Auto-generated destructor stub
}

bool ArbitraryMap::hasKey(char* key) {
	if(arbMap.find(key) != arbMap.end())
		return true;
	else
		return false;
}

void ArbitraryMap::setInt(char* key, int val) {
	int* intPtr = static_cast<int*>(malloc(sizeof(int)));
	*intPtr = val;

	arbMap[key] = (void*)intPtr;
}

int ArbitraryMap::getInt(char* key) {
	int* intPtr = static_cast<int*>(arbMap[key]);
	return *intPtr;
}

void ArbitraryMap::setDouble(char* key, double val) {
	double* doublePtr = static_cast<double*>(malloc(sizeof(double)));
	*doublePtr = val;

	arbMap[key] = (void*)doublePtr;
}

double ArbitraryMap::getDouble(char* key) {
	double* doublePtr = static_cast<double*>(arbMap[key]);
	return *doublePtr;
}

void ArbitraryMap::setBool(char* key, bool val) {
	bool* boolPtr = static_cast<bool*>(malloc(sizeof(bool)));
	*boolPtr = val;

	arbMap[key] = (void*)boolPtr;
}

bool ArbitraryMap::getBool(char* key) {
	bool* boolPtr = static_cast<bool*>(arbMap[key]);
	return *boolPtr;
}

void ArbitraryMap::setPtr(char* key, void* val) {
	arbMap[key] = val;
}

void* ArbitraryMap::getPtr(char* key) {
	return arbMap[key];
}
