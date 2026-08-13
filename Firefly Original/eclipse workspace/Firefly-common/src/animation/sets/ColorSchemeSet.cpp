/*
 * ColorSchemeSet.cpp
 *
 *  Created on: Aug 25, 2016
 *      Author: d
 */

#include <animation/sets/ColorSchemeSet.h>


ColorSchemeSet::ColorSchemeSet() {
}

ColorSchemeSet::~ColorSchemeSet() {
	// TODO Auto-generated destructor stub
}

void ColorSchemeSet::add(int creationIndex, double weight) {
	weightedSet[weight+totalWeight] = creationIndex;
	totalWeight += weight;
}

AColorScheme* ColorSchemeSet::next() {
	if(weightedSet.size() == 0) {
		return NULL;
	}
	double choice = rand1()*totalWeight;
	return createIndex(weightedSet.upper_bound(choice)->second);
}
