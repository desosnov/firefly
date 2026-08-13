/*
 * AnimationSet.h
 *
 *  Created on: Aug 10, 2016
 *      Author: Denis
 */

#ifndef ANIMATION_SETS_ANIMATIONSET_H_
#define ANIMATION_SETS_ANIMATIONSET_H_

#include <animation/SpheresAnimation.h>
#include <animation/BalloonsAnimation.h>
#include <animation/BalloonsAtPixelsAnimation.h>
#include <map>

class AnimationSet {
protected:
	PixelStage* stage;
	std::map<double, int> weightedSet;
	double totalWeight = 0.0;
	void add(int creationIndex, double weight);

	virtual AAnimation* createIndex(int index) =0;

public:
	AnimationSet(PixelStage* stage)
		: stage(stage)
	{};
	virtual ~AnimationSet();

	AAnimation* next();
};

class AllAnimations : public AnimationSet {
protected:
	AAnimation* createIndex(int index) {
		switch(index) {
			case 0: printf("[AllAnimations] Return SpheresAnimation\n"); return new SpheresAnimation(stage);
			case 1: printf("[AllAnimations] Return BalloonsAnimation\n"); return new BalloonsAnimation(stage);
			case 2: printf("[AllAnimations] Return BalloonsAtPixelsAnimation\n"); return new BalloonsAtPixelsAnimation(stage);
			default: printf("[AllAnimations] Return default SpheresAnimation\n"); return new SpheresAnimation(stage);
		}
	}

public:
	AllAnimations(PixelStage* stage)
		: AnimationSet(stage)
	{
		add(0, 3.0); // SpheresAnimation
		add(1, 1.0); // BalloonsAnimation
		add(2, 1.0); // BalloonsAtPixelsAnimation
	};
};

#endif /* ANIMATION_SETS_ANIMATIONSET_H_ */

