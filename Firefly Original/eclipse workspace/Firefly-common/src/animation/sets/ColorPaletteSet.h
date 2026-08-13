/*
 * ColorPaletteSet.h
 *
 *  Created on: Aug 25, 2016
 *      Author: d
 */

#ifndef SRC_ANIMATION_SETS_COLORPALETTESET_H_
#define SRC_ANIMATION_SETS_COLORPALETTESET_H_

#include <animation/base/AColorPalette.h>
#include <animation/colors/palettes/NarrowHueRangePalette.h>
#include <animation/colors/palettes/RandomDesaturatedPalette.h>
#include <animation/colors/palettes/RandomSaturatedPalette.h>
#include <animation/colors/palettes/SingleRandomHuePalette.h>
#include <animation/colors/palettes/TwoRandomHuesPalette.h>
#include <animation/colors/palettes/WideHueRangePalette.h>
#include <stdio.h>
#include <map>
#include "FireflyUtils.h"

class ColorPaletteSet {
protected:
	std::map<double, int> weightedSet;
	double totalWeight = 0.0;
	void add(int creationIndex, double weight);

	virtual AColorPalette* createIndex(int index) =0;

public:
	ColorPaletteSet();
	virtual ~ColorPaletteSet();

	AColorPalette* next();
};

class AllColorPalettes : public ColorPaletteSet {
protected:
	AColorPalette* createIndex(int index) {
		switch(index) {
			case 0: printf("[AllColorPalettes] Return RandomSaturatedPalette\n"); return new RandomSaturatedPalette();
			case 1: printf("[AllColorPalettes] Return RandomDesaturatedPalette\n"); return new RandomDesaturatedPalette();
			case 2: printf("[AllColorPalettes] Return SingleRandomHuePalette\n"); return new SingleRandomHuePalette();
			case 3: printf("[AllColorPalettes] Return TwoRandomHuesPalette\n"); return new TwoRandomHuesPalette();
			case 4: printf("[AllColorPalettes] Return NarrowHueRangePalette\n"); return new NarrowHueRangePalette();
			case 5: printf("[AllColorPalettes] Return WideHueRangePalette\n"); return new WideHueRangePalette();
			default: printf("[AllColorPalettes] Return default RandomSaturatedPalette\n"); return new RandomSaturatedPalette();
		}
	}

public:
	AllColorPalettes() {
		add(0, 2.0); // RandomSaturatedPalette
		add(1, 2.0); // RandomDesaturatedPalette
		add(2, 1.0); // SingleRandomHuePalette
		add(3, 2.0); // TwoRandomHuesPalette
		add(4, 2.0); // NarrowHueRangePalette
		add(5, 2.0); // WideHueRangePalette
	};
};

#endif /* SRC_ANIMATION_SETS_COLORPALETTESET_H_ */
