/*
 * ColorSchemeSet.h
 *
 *  Created on: Aug 25, 2016
 *      Author: d
 */

#ifndef SRC_ANIMATION_SETS_COLORSCHEMESET_H_
#define SRC_ANIMATION_SETS_COLORSCHEMESET_H_

#include <animation/colors/schemes/GradientToBlackScheme.h>
#include <animation/colors/schemes/GradientToTransparentScheme.h>
#include <animation/colors/schemes/RandomGradientScheme.h>
#include <animation/colors/schemes/SolidColorsScheme.h>
#include <stdio.h>
#include <map>
#include "FireflyUtils.h"

class ColorSchemeSet {
protected:
	std::map<double, int> weightedSet;
	double totalWeight = 0.0;
	void add(int creationIndex, double weight);

	virtual AColorScheme* createIndex(int index) =0;

public:
	ColorSchemeSet();
	virtual ~ColorSchemeSet();

	AColorScheme* next();
};

class AllColorSchemes : public ColorSchemeSet {
protected:
	AColorScheme* createIndex(int index) {
		switch(index) {
			case 0: printf("[AllColorSchemes] Return SolidColorsScheme\n"); return new SolidColorsScheme();
			case 1: printf("[AllColorSchemes] Return RandomGradientScheme\n"); return new RandomGradientScheme();
			case 2: printf("[AllColorSchemes] Return GradientToBlackScheme\n"); return new GradientToBlackScheme();
			case 3: printf("[AllColorSchemes] Return GradientToTransparentScheme\n"); return new GradientToTransparentScheme();
			default: printf("[AllColorSchemes] Return default SolidColorsScheme\n"); return new SolidColorsScheme();
		}
	}

public:
	AllColorSchemes() {
		add(0, 1.0); // SolidColorsScheme
		add(1, 2.0); // RandomGradientScheme
		add(2, 2.0); // GradientToBlackScheme
		add(3, 2.0); // GradientToTransparentScheme
	};
};

#endif /* SRC_ANIMATION_SETS_COLORSCHEMESET_H_ */
