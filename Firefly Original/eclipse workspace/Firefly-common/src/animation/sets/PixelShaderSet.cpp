/*
 * PixelShaderSet.cpp
 *
 *  Created on: Aug 12, 2016
 *      Author: d
 */

#include <animation/sets/PixelShaderSet.h>
#include "FireflyUtils.h"

PixelShaderSet::PixelShaderSet() {
	// TODO Auto-generated constructor stub

}

PixelShaderSet::~PixelShaderSet() {
	// TODO Auto-generated destructor stub
}

void PixelShaderSet::add(int creationIndex, double weight) {
	weightedSet[weight+totalWeight] = creationIndex;
	totalWeight += weight;
}

APixelShader* PixelShaderSet::next() {
	if(weightedSet.size() == 0) {
		return NULL;
	}

	double choice = rand1()*totalWeight;
	return createIndex(weightedSet.upper_bound(choice)->second);
}
