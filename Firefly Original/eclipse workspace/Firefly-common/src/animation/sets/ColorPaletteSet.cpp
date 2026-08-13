/*
 * ColorPaletteSet.cpp
 *
 *  Created on: Aug 25, 2016
 *      Author: d
 */

#include <animation/sets/ColorPaletteSet.h>

ColorPaletteSet::ColorPaletteSet() {
	// TODO Auto-generated constructor stub

}

ColorPaletteSet::~ColorPaletteSet() {
	// TODO Auto-generated destructor stub
}

void ColorPaletteSet::add(int creationIndex, double weight) {
	weightedSet[weight+totalWeight] = creationIndex;
	totalWeight += weight;
}

AColorPalette* ColorPaletteSet::next() {
	if(weightedSet.size() == 0) {
		return NULL;
	}
	double choice = rand1()*totalWeight;
	return createIndex(weightedSet.upper_bound(choice)->second);
}
