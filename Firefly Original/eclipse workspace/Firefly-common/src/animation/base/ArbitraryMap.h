/*
 * ArbitraryMap.h
 *
 *  Created on: Aug 13, 2016
 *      Author: d
 */

#ifndef SRC_ANIMATION_BASE_ARBITRARYMAP_H_
#define SRC_ANIMATION_BASE_ARBITRARYMAP_H_

#include <stdlib.h>
#include <map>

class ArbitraryMap {
private:
	std::map<char*, void*> arbMap;

public:
	ArbitraryMap();
	virtual ~ArbitraryMap();

	bool hasKey(char* key);
	void setInt(char* key, int val);
	int getInt(char* key);
	void setDouble(char* key, double val);
	double getDouble(char* key);
	void setBool(char* key, bool val);
	bool getBool(char* key);
	void setPtr(char* key, void* val);
	void* getPtr(char* key);
};

#endif /* SRC_ANIMATION_BASE_ARBITRARYMAP_H_ */
