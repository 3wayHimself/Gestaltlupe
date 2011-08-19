﻿using System;
using System.Collections.Generic;
using System.Text;

using Fractrace.Basic;

namespace Fractrace {
  class GlobalParameters {


    /// <summary>
    /// Hier können vom Programm aus globale Parameter hinzugefügt werden.
    /// </summary>
    public static void SetGlobalParameters() {

      // Not used anymore:
     // ParameterDict.Exemplar["View.Zoom"] = "1";

      // Minimal X-Value.
      ParameterDict.Exemplar["Border.Min.x"] = "-1.5";
      // Maximal X-Value.
      ParameterDict.Exemplar["Border.Max.x"] = "1.5";
      // Minimal Y-Value.
      ParameterDict.Exemplar["Border.Min.y"] = "-1.5";
      // Maximal y-Value.
      ParameterDict.Exemplar["Border.Max.y"] = "1.5";
      // Minimal Z-Value
      ParameterDict.Exemplar["Border.Min.z"] = "-1.5";
      // Maximal Z-Value.
      ParameterDict.Exemplar["Border.Max.z"] = "1.5";

      // Not used anymore
      ParameterDict.Exemplar["Border.Min.zz"] = "-1.5";
      // Not used anymore
      ParameterDict.Exemplar["Border.Max.zz"] = "1.5";

      // Rotation angle (in degree) for axis x (center=(0,0,0)).
      ParameterDict.Exemplar["Transformation.AngleX"] = "0";
      // Rotation angle (in degree) for axis y (center=(0,0,0)).
      ParameterDict.Exemplar["Transformation.AngleY"] = "0";
      // Rotation angle (in degree) for axis z (center=(0,0,0)).
      ParameterDict.Exemplar["Transformation.AngleZ"] = "0";

      // Rotation angle (in degree) for axis x (rotation center is center of the given bounds).
      ParameterDict.Exemplar["Transformation.Camera.AngleX"] = "0";
      // Rotation angle (in degree) for axis y (rotation center is center of the given bounds).
      ParameterDict.Exemplar["Transformation.Camera.AngleY"] = "0";
      // Rotation angle (in degree) for axis z (rotation center is center of the given bounds).
      ParameterDict.Exemplar["Transformation.Camera.AngleZ"] = "0";

      // Rotation angle (in degree) for axis x (center=(Transformation.3.CenterX,Transformation.3.CenterY,Transformation.3.CenterZ)).
      ParameterDict.Exemplar["Transformation.3.AngleX"] = "0";
      // Rotation angle (in degree) for axis y (center=(Transformation.3.CenterX,Transformation.3.CenterY,Transformation.3.CenterZ)).
      ParameterDict.Exemplar["Transformation.3.AngleY"] = "0";
      // Rotation angle (in degree) for axis z (center=(Transformation.3.CenterX,Transformation.3.CenterY,Transformation.3.CenterZ)).
      ParameterDict.Exemplar["Transformation.3.AngleZ"] = "0";

      // x- component of the center of rotation Transformation.3
      ParameterDict.Exemplar["Transformation.3.CenterX"] = "0";
      // y- component of the center of rotation Transformation.3
      ParameterDict.Exemplar["Transformation.3.CenterY"] = "0";
      // z- component of the center of rotation Transformation.3
      ParameterDict.Exemplar["Transformation.3.CenterZ"] = "0";

      // Distance to the virtual screen. Small values gives a more 3D effect. Large values
      // gives the scene a parallel projection view.
      ParameterDict.Exemplar["Transformation.Perspective.Cameraposition"] = "0.4";

      // X-component of the Julia Seed, if the formula is in julia mode.
      // X-component of the start value , if the formula is in mandelbrot mode.
      ParameterDict.Exemplar["Formula.Static.jx"] = "0";
      // Y-component of the Julia Seed, if the formula is in julia mode.
      // Y-component of the start value , if the formula is in mandelbrot mode.
      ParameterDict.Exemplar["Formula.Static.jy"] = "0";
      // Z-component of the Julia Seed, if the formula is in julia mode.
      // Z-component of the start value , if the formula is in mandelbrot mode.
      ParameterDict.Exemplar["Formula.Static.jz"] = "0";
      // Q-component of the Julia Seed, if the formula is in julia mode.
      ParameterDict.Exemplar["Formula.Static.jzz"] = "0";

      // Number of iterations used in the formula.
      ParameterDict.Exemplar["Formula.Static.Cycles"] = "12";

      // Number of iterations used in the formula, if the inside of the 3D object is rendered.
      // Caution: If minCycle=51, then inside rendering is deactivated.
      ParameterDict.Exemplar["Formula.Static.MinCycle"] = "51";
      // The used formula.
      // Values from 1 to 26 corresponds to some inbuild formulas.
      // Formula.Static.Formula=-1: Use "Intern.Formula.Source" in mandelbrot mode.
      // Formula.Static.Formula=-2: Use "Intern.Formula.Source" in julia mode.
      ParameterDict.Exemplar["Formula.Static.Formula"] = "-1";

      // Source code of the formula, if Formula.Static.Formula < 0.
      ParameterDict.Exemplar["Intern.Formula.Source"] = @"
public override void Init() { 
  base.Init();
  additionalPointInfo=new AdditionalPointInfo();
  gr1=GetDouble(""Formula.Static.Cycles"");
  int tempGr=(int)gr1;
  gr1=gr1- tempGr;
  gr1=1-gr1;
  gr1*=2.4;
}

double gr1=0;

public override long InSet(double ar, double ai, double aj,  double br, double bi, double bj, double bk, long zkl, bool invers) {
  double aar, aai, aaj;
  long tw;
  int n;
  int pow = 8; // n=8 default Mandelbulb
  double gr =Math.Pow(10,gr1)+1.0;  // Bailout
  double theta, phi;
  double r_n = 0;
  aar = ar * ar; aai = ai * ai; aaj = aj * aj;
  tw = 0L;
  double r = Math.Sqrt(aar + aai + aaj);
  double  phi_pow,theta_pow,sin_theta_pow,rn_sin_theta_pow;

  additionalPointInfo.red=0;
  additionalPointInfo.green=0;
  additionalPointInfo.blue=0;

  for (n = 1; n < zkl; n++) {
    theta = Math.Atan2(Math.Sqrt(aar + aai), aj);
    phi = Math.Atan2(ai, ar);
    r_n = Math.Pow(r, pow);
    phi_pow=phi*pow;
    theta_pow=theta*pow;
    sin_theta_pow=Math.Sin(theta_pow);
    rn_sin_theta_pow=r_n* sin_theta_pow;
    ar =  rn_sin_theta_pow * Math.Cos(phi_pow)+br;
    ai = rn_sin_theta_pow * Math.Sin(phi_pow)+bi;
    aj = r_n * Math.Cos(theta_pow)+bj;
    aar = ar * ar; aai = ai * ai; aaj = aj * aj;
    r = Math.Sqrt(aar + aai + aaj);
    additionalPointInfo.red=aar;
    additionalPointInfo.green=aai;
    additionalPointInfo.blue=aaj;
    if (r > gr) { tw = n; break; }
  }

  if (invers) {
     if (tw == 0)
        tw = 1;
      else
        tw = 0;
  }
  return (tw);
}


";

      // Rendering Quality.
      // View.Raster=1: best quality
      // View.Raster=2, up to 8 times faster than View.Raster=1, but less nice - especially at the object borders. 
      ParameterDict.Exemplar["View.Raster"] = "2";

      // View.Size*View.Width == width of the rendered bitmap.
      ParameterDict.Exemplar["View.Size"] = "1";

      // Set "View.ClassicView" to 1 to activate the classic view ... eventually (not tested anymore)
      ParameterDict.Exemplar["View.ClassicView"] = "0";

      // Switch between 3D view and parallel view.
      ParameterDict.Exemplar["View.Perspective"] = "1";

      // View.Size*View.Width == width of the rendered bitmap.
      ParameterDict.Exemplar["View.Width"] = "1200";

      // View.Size*View.Height == height of the rendered bitmap.
      ParameterDict.Exemplar["View.Height"] = "1200";

      // Virtual voxel space at the y-coordinate. Higher values :-> more accurate rendering, but 
      // more time consuming.
      ParameterDict.Exemplar["View.Deph"] = "800";

      // Additional voxel space (removes black background parts of the rendered image).
      ParameterDict.Exemplar["View.DephAdd"] = "0";

      // Used internally in creating a poster.
      // PosterX=-1: Render the bitmap left the given bounds.
      // PosterX=1: Render the bitmap right the given bounds.
      ParameterDict.Exemplar["View.PosterX"] = "0";
      // Used internaly in creating of a poster.
      // PosterZ=-1: Render the bitmap above the given bounds.
      // PosterZ=1: Render the bitmap under the given bounds.
      ParameterDict.Exemplar["View.PosterZ"] = "0";

      // Default animation steps while adding the frame in the animation control. 
      ParameterDict.Exemplar["Animation.Steps"] = "30";

      // Strength of blurring (0-1)  [used in Renderer 2-4]
      ParameterDict.Exemplar["Composite.Blurring"] = "0";

      // not used anymore.
      ParameterDict.Exemplar["Composite.BlurringDeph"] = "0";

      // Activates the darkening of distand points    [used in Renderer 2,3,4,6]
      ParameterDict.Exemplar["Composite.BackgoundDarken"] = "0";
      // Light and material property  [used in Renderer 2 - 4]
      ParameterDict.Exemplar["Composite.Shininess"] = "1";
      // Activates the Blurring.  [used in Renderer ?]
      ParameterDict.Exemplar["Composite.UseAmbient"] = "1";
      // Activates the darkening of distand points    [used in Renderer ?]
      ParameterDict.Exemplar["Composite.UseDarken"] = "1";
      // Use object median instead of the center of the bounds [used in Renderer 2,3] 
      ParameterDict.Exemplar["Composite.UseMedian"] = "1";
      //  [used in Renderer ?]
      ParameterDict.Exemplar["Composite.UseDerivation"] = "1";
      // Activates the surface color [used in Renderer ?] 
      ParameterDict.Exemplar["Composite.UseColor1"] = "0";
      // [used in Renderer ?]
      ParameterDict.Exemplar["Composite.Color1Factor"] = "50";
      // [used in Renderer ?]
      ParameterDict.Exemplar["Composite.Color1TestArea"] = "10";
      // Activates additional front light  [used in Renderer ?]
      ParameterDict.Exemplar["Composite.FrontLight"] = "1";
      // Activates additional ambient light  [used in Renderer ?]
      ParameterDict.Exemplar["Composite.AmbientLight"] = "0";
      // Normalise the color and light parameter to get the best picture results.
      // Don't use this parameter in animations [used in Renderer 2 - 5]
      ParameterDict.Exemplar["Composite.Normalize"] = "1";

      // Set the Renderer:
      // Composite.Renderer=0 : use FastScienceRenderer
      // Composite.Renderer=1 : use ScienceRenderer
      // Composite.Renderer=2 : use NiceRenderer
      // Composite.Renderer=3 : use BroadcastRenderer
      // Composite.Renderer=4 : use SharpRenderer
      // Composite.Renderer=5 : use UniversalRenderer
      // Composite.Renderer=6 : use PlasicRenderer
      ParameterDict.Exemplar["Composite.Renderer"] = "PlasicRenderer";


      // Activates Blurring in UniversalRenderer
      ParameterDict.Exemplar["Composite.Renderer.Universal.UseAmbient"] = "1";
      // Activates Background darkening in UniversalRenderer
      ParameterDict.Exemplar["Composite.Renderer.Universal.UseDarken"] = "1";
      // Activates surface color in UniversalRenderer
      ParameterDict.Exemplar["Composite.Renderer.Universal.UseColorFromFormula"] = "1";
      // Activates median for surface color in UniversalRenderer
      ParameterDict.Exemplar["Composite.Renderer.Universal.UseMedianColorFromFormula"] = "1";
      // Activates comic style (additional black lines at the border) in UniversalRenderer
      ParameterDict.Exemplar["Composite.Renderer.Universal.ComicStyle"] = "0";
      // Intensity (0-1) of the Front Light in UniversalRenderer.
      ParameterDict.Exemplar["Composite.Renderer.Universal.FrontLightIntensity"] = "0.1";
      // Intensity (0-1) of the Ambient Light in UniversalRenderer.
      ParameterDict.Exemplar["Composite.Renderer.Universal.AmbientLightIntensity"] = "0.4";
      // Additional Brightening (0-1) in UniversalRenderer
      ParameterDict.Exemplar["Composite.Renderer.Universal.Brightening"] = "0";
      // Normalise the color and light parameter to get the best picture results.
      // Don't use this parameter in animations [used in UniversalRenderer]
      ParameterDict.Exemplar["Composite.Renderer.Universal.NormalizeColors"] = "0";

      // Activates Background darkening in PlasicRenderer
      ParameterDict.Exemplar["Composite.Renderer.Plasic.UseDarken"] = "0";
      // Corresponds to the number of shadows in PlasicRenderer
      ParameterDict.Exemplar["Composite.Renderer.Plasic.ShadowNumber"] = "12";
      // Intensity of the FieldOfView
      ParameterDict.Exemplar["Composite.Renderer.Plasic.AmbientIntensity"] = "1";
      // Minimal value of FieldOfView
      ParameterDict.Exemplar["Composite.Renderer.Plasic.MinFieldOfView"] = "0.3";
      // Maximal value of FieldOfView
      ParameterDict.Exemplar["Composite.Renderer.Plasic.MaxFieldOfView"] = "1";

      // Intensity of the Surface Color
      ParameterDict.Exemplar["Composite.Renderer.Plasic.ColorIntensity"] = "1";
      // If ColorGreyness=1, no color is rendered
      ParameterDict.Exemplar["Composite.Renderer.Plasic.ColorGreyness"] = "0";
      // Use Light
      ParameterDict.Exemplar["Composite.Renderer.Plasic.UseLight"] = "1";
      // Shadow height factor
      ParameterDict.Exemplar["Composite.Renderer.Plasic.ShadowJustify"] = "2";
      // Shininess factor (0 ... 1)
      ParameterDict.Exemplar["Composite.Renderer.Plasic.ShininessFactor"] = "0.3";
      // Shininess ( 0... 1000)
      ParameterDict.Exemplar["Composite.Renderer.Plasic.Shininess"] = "5";
     // Normal of the light source
      ParameterDict.Exemplar["Composite.Renderer.Plasic.Light.X"] = "0.45";
      ParameterDict.Exemplar["Composite.Renderer.Plasic.Light.Y"] = "1";
      ParameterDict.Exemplar["Composite.Renderer.Plasic.Light.Z"] = "0.45";
      // Set to 1 to enable sharp shadow rendering (warning: time consuming) 
      ParameterDict.Exemplar["Composite.Renderer.Plasic.UseSharpShadow"] = "0";
     // To justify the color components 
      ParameterDict.Exemplar["Composite.Renderer.Plasic.ColorFactor.Red"] = "1";
      ParameterDict.Exemplar["Composite.Renderer.Plasic.ColorFactor.Green"] = "1";
      ParameterDict.Exemplar["Composite.Renderer.Plasic.ColorFactor.Blue"] = "1";
      // accepted integer values: 1, ..., 6  (all values>1 :switch rgb components)
      ParameterDict.Exemplar["Composite.Renderer.Plasic.ColorFactor.RgbType"] = "1";

      // If LightIntensity==1, no shadow renderers are used
      // If LightIntensity==0, only shadow renderers are used
      ParameterDict.Exemplar["Composite.Renderer.Plasic.LightIntensity"] = "0.5";

      // Number of threads used in computation. The recommended value is the number of processors.
      ParameterDict.Exemplar["Computation.NoOfThreads"] = Environment.ProcessorCount.ToString();

      // EyeDistance in stereo mode.
      ParameterDict.Exemplar["Transformation.Stereo.EyeDistance"] = "0.5";
      // Angle difference in stereo mode. 
      ParameterDict.Exemplar["Transformation.Stereo.Angle"] = "-9";
    }
  }
}
