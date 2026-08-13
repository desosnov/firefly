/*
 * AnimationSet.cpp
 *
 *  Created on: Aug 10, 2016
 *      Author: Denis
 */

#include <animation/sets/AnimationSet.h>

AnimationSet::~AnimationSet() {
	// TODO Auto-generated destructor stub
}

void AnimationSet::add(int creationIndex, double weight) {
	weightedSet[weight+totalWeight] = creationIndex;
	totalWeight += weight;
}

AAnimation* AnimationSet::next() {
	if(weightedSet.size() == 0) {
		return NULL;
	}

	double choice = rand1()*totalWeight;
	return createIndex(weightedSet.upper_bound(choice)->second);
}
