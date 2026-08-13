/*
 * WorldOfBalloonsAnimation.h
 *
 *  Created on: Aug 9, 2016
 *      Author: Denis
 */

#ifndef ANIMATION_WORLDOFBALLOONSANIMATION_H_
#define ANIMATION_WORLDOFBALLOONSANIMATION_H_

#include <animation/base/AAnimation.h>
#include <animation/base/AEasingFunction1D.h>
#include <animation/colors/palettes/RandomSaturatedPalette.h>
#include <animation/easing/CosineEase1D.h>
#include <animation/easing/CosineEase3D.h>
#include <animation/primitives/SpherePrim.h>
#include <glm/detail/type_vec.hpp>
#include <vector>

#define WOB_MIN_SIZE 0.05
#define WOB_MAX_SIZE 1.0
#define WOB_MIN_GROW_TIME 2.5
#define WOB_MAX_GROW_TIME 15.0

#define WOB_STAGE_SIZE 1.5
#define WOB_SIMULT_SPHERES_GROWING 20
#define WOB_MAX_SPHERES 100

#define WOB_WRAPUP_DURATION 10.0
#define WOB_WRAPUP_MOVE_TIME 5.0

#define WOB_ALPHA_RENDER_CUTOFF 0.97

class BalloonsAnimation: public AAnimation {
protected:
	std::vector<SpherePrim*> spheres;
	long numSpheres = 0;
	std::vector<AEasingFunction1D*> growthEases;
	std::vector<AEasingFunction3D*> wrapUpEases;

	virtual SpherePrim* newSphere();
	virtual AEasingFunction1D* newGrowthEase(double time);
	virtual void addSphere(double time);

	bool wrappingUp = false, nextAnimationFlag = false, finishedFlag = false;
	double nextAnimationTime = 0.0, finishTime = 0.0;

	virtual void initInternal();
	virtual void updateInternal(double time);

public:
	BalloonsAnimation(
			PixelStage* stage,
			AColorPalette *palette = new RandomSaturatedPalette(),
			AColorScheme *scheme = new SolidColorsScheme())
			: AAnimation(stage, palette, scheme)
	{
		subpixelSampling = false;
	};

	virtual glm::vec4 renderPixelInternal(glm::vec3 pos, ArbitraryMap* details);

	void beginWrappingUp();
	bool readyForNextAnimation();
	bool finished();

};

#endif /* ANIMATION_WORLDOFBALLOONSANIMATION_H_ */
