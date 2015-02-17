////////////////////////////////////////////////////////////////////
//                                                                  
//         _____________    ____       ____      __________         
//        //////////////\  /////\     /////\    ///////////\        
//        \______////   / ///// /    ///// /  ////// __//// /       
//             ////   /  ///// /    ///// /  (///// / //// /        
//           ////   /   //////////////// /    \////\  \___/         
//         ////   /    //////////////// / ___  \////\               
//       ////   /     ///// _____///// / ////\  \//// )             
//     ////   /      ///// /    ///// / ////_/_///// /              
//  //////////////\ ///// /    ///// / |//////////  /               
//  \_____________/ \___\/     \___\/   \__________/ cript          
//                                                                  
//                                                                  
//  Standard Common Scripts for ZHScript v1.6                       
//                                                                  
//      Date: 01-27-15                                              
//                                                                  
////////////////////////////////////////////////////////////////////

/*

*/

// Global scope variables
var level : GameLevel = null;
var player : Player = null;

var gameFPS : int = 0;

var screenWidth : int = 0;
var screenHeight : int = 0;

var windowWidth : int = 0;
var windowHeight : int = 0;

var device : GraphicsDevice = null;

var font_default : SpriteFont = null;
var font_default_bold : SpriteFont = null;
var font_small : SpriteFont = null;
var font_small_bold : SpriteFont = null;
var font_large : SpriteFont = null;
var font_large_bold : SpriteFont = null;
var font_extra_small : SpriteFont = null;
var font_extra_small_bold : SpriteFont = null;

let DEBUG : bool = true;

let PI : float = 3.141592653589793;

// Whether to process input normally
var allowInput : bool = true;

var timer : int = 0;

// Some of the functions are just higher wrappers to lower functions
@init()
@step(time : float)
@end()

@trace(args : any...)
@startSequence(seq : any)
@stopSequence(seq : any)
@error(errorMessage : string)

// Runtime related functions
@rerun()
@pause()

// Math related functions
@sin(x : float) : float
@cos(x : float) : float
@tan(x : float) : float
@atan(x : float) : float
@atan2(x : float, y : float) : float
@asin(x : float) : float
@acos(x : float) : float
@min(x : float, y : float) : float
@max(x : float, y : float) : float
@pow(x : float, y : float) : float
@abs(x : float) : float
@toInt(x : float) : float
@toFloat(x : float) : float
@toShort(x : float) : float
@toUint(x : float) : float

// Reflection related functions
@getType(typeName : string) : System.Type
@typeOf(o : any) : System.Type
@callMethod(o : any, method : any, args : [any] = null) : any
@obj(name : string, args : [any] = null) : objectDef
@setField(o : any, field : string, value : any) : any
@setProp(o : any, prop : string, value : any) : any
@getField(o : any, field : string) : any
@getProp(o : any, prop : string) : any

// Trigger functions
@removeTrigger(trig : string, o : any = null) : void
@resetTriggers() : void

// Misc low level functions
@funcExists(f : string, localsOnly : bool = false) : bool
@__hashtable() : Hashtable
@__timeout(funcName : string, timeout : int, args : [any], expires : bool = false) : TriggerWatch
@__objecttimeout(inst : object, f : string, timeout : int, args : [any] = null, expires : bool = false) : TriggerWatch

///////////////////////////////////////////////
// 
// DEFAULT ENTRY POINT FOR THE SCRIPT
// 
///////////////////////////////////////////////
// 
// levelRef should be the GameLevel that is currently running the scripts
func __init(levelRef : GameLevel)
{
    ///// Define global variables
    
    // Display some debug information
    if(DEBUG)
    {
        trace("|---------------------------------------------|");
        trace("|                                             |");
        trace("|                                             |");
        trace("|           ZombieHouse Script v1.5           |");
        trace("|                                             |");
        trace("|       Compiler/Runner Date: 01-27-15        |");
        trace("|               Commons Date: 01-27-15        |");
        trace("|                                             |");
        trace("|---------------------------------------------|");
        trace("Initializing global variables...");
    }
    
    level = levelRef;
    player = level.player;
    
    device = level.MainEngine.Device;
    
    gameFPS = (int)(1000.0 / level.MainEngine.Game.TargetElapsedTime.TotalMilliseconds);
    
    screenWidth  = level.MainEngine.Width;
    screenHeight = level.MainEngine.Height;
    
    windowWidth  = level.MainEngine.Device.Viewport.Width;
    windowHeight = level.MainEngine.Device.Viewport.Height;
    
    // Get the font objects
    font_default          = getStatic('Zombie_House.Statics', 'DEFAULT_FONT');
    font_default_bold     = getStatic('Zombie_House.Statics', 'DEFAULT_FONT_BOLD');
    font_small            = getStatic('Zombie_House.Statics', 'DEFAULT_FONT_SMALL');
    font_small_bold       = getStatic('Zombie_House.Statics', 'DEFAULT_FONT_SMALL_BOLD');
    font_large            = getStatic('Zombie_House.Statics', 'DEFAULT_FONT_LARGE');
    font_large_bold       = getStatic('Zombie_House.Statics', 'DEFAULT_FONT_LARGE_BOLD');
    font_extra_small      = getStatic('Zombie_House.Statics', 'DEFAULT_FONT_EXTRA_SMALL');
    font_extra_small_bold = getStatic('Zombie_House.Statics', 'DEFAULT_FONT_EXTRA_SMALL_BOLD');
    
    if(DEBUG)
    {
        trace("Initialized successfully.");

        if(windowWidth == screenWidth && windowHeight == screenHeight)
        {
            trace("Running on " + screenWidth + "x" + screenHeight + " @ " + gameFPS + " frames per second.");
        }
        else
        {
            trace("Running on " + screenWidth + "x" + screenHeight + " (window size: " + windowWidth + "x" + windowHeight + ") @ " + gameFPS + " frames per second.");
        }
    }
    
    // Call the secondary main init function
    if(funcExists('init', true))
        init();
}
func __step(time : float)
{
    timer += time;
    
    if(getStaticProp("Zombie_House.Entities.UI.OutputPanel", "Panel") == null || getStaticProp("Zombie_House.Entities.UI.OutputPanel", "Panel").Show)
        allowInput = false;
    else
        allowInput = true;
    
    if(funcExists('step', true))
        step(time);
}
func __end()
{
    if(funcExists('end', true))
        end();
}

// Sets a timeout before firing the given function with the given parameters
func setTimeout(f : string, time : int, params:[any] = null, expires:bool = true) : TriggerWatch
{
    return __timeout(f, time, params, expires);
}

// Sets a timeout before firing the given object function with the given parameters
func setObjectTimeout(_obj : object, funcName : string, time : int, params=null, expires=true) : TriggerWatch
{
    return __objecttimeout(_obj, funcName, time, params, expires);
}

// Interpolates the given property of an object over the given frames
func interpolate(_obj : any, prop : string, type : string, interp : [any], interpDelay : float, cInterp : float = 0.0)
{
    if(cInterp >= interpDelay)
    {
        if(type == 'field')
            setField(_obj, prop, interp[1]);
        else if(type == 'prop')
            setProp(_obj, prop, interp[1]);
        
        return;
    }
    
    if(type == 'field')
        setField(_obj, prop, interp[1] - (interp[1] - interp[0]) * (1 - cInterp / interpDelay));
    else if(type == 'prop')
        setProp(_obj, prop, interp[1] - (interp[1] - interp[0]) * (1 - cInterp / interpDelay));
    
    setTimeout('interpolate', 1, [_obj, prop, type, [interp[0], interp[1]], interpDelay, cInterp + 1]);
}

// Transforms the given frames value into seconds
func framesToSeconds(frames : int) : int
{
    return (frames / gameFPS);
}
// Transforms the given seconds value into frames
func secondsToFrames(seconds : int) : int
{
    return (gameFPS * seconds);
}

func inspect(_obj : any) : void
{
    if(_obj == null)
    {
        trace("Cannot inspect a null object!");
        return;
    }
    
    trace("Typeof: " + _obj.GetType());
    trace();
    
    trace("METHODS:");
    inspectMethods(_obj);
    trace();
    
    trace("FIELDS:");
    inspectFields(_obj);
}

func inspectMethods(_obj : any, method : any = null)
{
    if(_obj == null)
        return;
    
    var type = _obj.GetType();
    var methods = type.GetMethods();
    if(method == null)
    {
        for(var i = 0; i < methods.Length; i++)
        {
            var compost = methods[i].Name + "(";
            
            for(var j = 0; j < methods[i].GetParameters().Length; j++)
            {
                if(j != 0)
                    compost += ", ";
                compost += methods[i].GetParameters()[j].ParameterType + " " + methods[i].GetParameters()[j].Name;
                
                if(methods[i].GetParameters()[j].IsOptional)
                {
                    if(methods[i].GetParameters()[j].RawDefaultValue == null)
                        compost += " = null";
                    else
                        compost += " = " + methods[i].GetParameters()[j].RawDefaultValue.ToString();
                }
            }
            compost += ")";
            trace(compost);
        }
    }
    else
    {
        for(var i = 0; i < methods.Length; i++)
        {
            if(methods[i].Name == method)
            {
                var compost = methods[i].Name + "(";
                
                for(var j = 0; j < methods[i].GetParameters().Length; j++)
                {
                    if(j != 0)
                        compost += ", ";
                    compost += methods[i].GetParameters()[j].ParameterType + " " + methods[i].GetParameters()[j].Name;
                    
                    if(methods[i].GetParameters()[j].IsOptional)
                    {
                        if(methods[i].GetParameters()[j].RawDefaultValue == null)
                            compost += " = null";
                        else
                            compost += " = " + methods[i].GetParameters()[j].RawDefaultValue.ToString();
                    }
                }
                compost += ")";
                trace(compost);
            }
        }
    }
}

func inspectFields(_obj : any, field : any = null) : void
{
    if(_obj == null)
        return;
    
    var type = _obj.GetType();
    var fields = type.GetFields();
    if(field == null)
    {
        for(var i = 0; i < fields.Length; i++)
        {
            var compost = fields[i].FieldType + " " + type.Name + "." + fields[i].Name + " = ";
            if(fields[i].GetValue(_obj) == null)
            {
                compost += "null";
            }
            else if(fields[i].GetValue(_obj) is System.String)
            {
                compost += "'" + fields[i].GetValue(_obj) + "'";
            }
            else
            {
                compost += fields[i].GetValue(_obj) + "";
            }
            
            trace(compost);
        }
    }
    else
    {
        for(var i = 0; i < fields.Length; i++)
        {
            if(fields[i].Name == field)
            {
                var compost = fields[i].FieldType + " " + type.Name + "." + fields[i].Name + " = ";
                if(fields[i].GetValue(_obj) == null)
                {
                    compost += "null";
                }
                else if(fields[i].GetValue(_obj) is System.String)
                {
                    compost += "'" + fields[i].GetValue(_obj) + "'";
                }
                else
                {
                    compost += fields[i].GetValue(_obj) + "";
                }
                
                trace(compost);
            }
        }
    }
}

// Objects
object ConstantBehavior
{
    func ConstantBehavior()
    {
        baseTimer = setObjectTimeout(this, 'step', 1, [], false);
    }
    
    func stopBehavior()
    {
        baseTimer.Triggered = true;
    }
    
    var baseTimer : TriggerWatch;
}

#include gamelevel_commons.zhs
#include player_commons.zhs
#include enemy_commons.zhs
#include network_commons.zhs

// Enemy related functions

// Spawns the given enemy object with the given parameters
func spawnEnemy(enemyObj, params=null) : any
{
    var zombieType = 0;
    
    switch(enemyObj.type)
    {
        case ENEMY_WEAKZOMBIE:
            zombieType = 0;
        break;
        case ENEMY_TOUGHZOMBIE:
            zombieType = 1;
        break;
        case ENEMY_BUILDINGZOMBIE:
            zombieType = 2;
        break;
        default:
            zombieType = 0;
        break;
    }
    
    let zx = params.x;
    let zy = params.y;
    var zd = params.d;
    
    if(zd == null)
        zd = 1;
    
    var zombie = level.spawnEnemy(zombieType, zx, zy, zd, -1);
    
    if(params.hp != null)
        zombie.TotalHP = zombie.HP = params.hp;
    else if(enemyObj.hp != null)
        zombie.TotalHP = zombie.HP = enemyObj.hp;
    
    return zombie;
}

// Damages the given enemy with the given ammount of damage, optionally specifying a damage source and redness modifier
// If the enemy's health reaches 0 or lower after the damage is dealt, the enemy is killed
func damageEnemy(enemy, damageAmm, damageSource = -2, rednessModifier = -1)
{
    enemy.Damage(damageAmm, damageSource, rednessModifier);
}
// Heals the given enemy with the given heal ammount, optionally specifying a heal source
func healEnemy(enemy, healAmm, healSource = -2)
{
    enemy.Heal(healAmm, healSource);
}
// Kills the given enemy, optionally specifying a kill source
func killEnemy(enemy = null, killSource = -2)
{
    enemy.Die(killSource);
}

let ENEMY_BUILDINGZOMBIE = 'BuildingZombie';
let ENEMY_WEAKZOMBIE = 'WeakZombie';
let ENEMY_TOUGHZOMBIE = 'ToughZombie';

// Loads a level
@loadLevel(levelName, levelFile=null)
// Quits the game
@quitGame()

#include game_commons.zhs

// Contains common game level commands
// All XNA-related functions and variables are stored in this file

// Low-level external functions
@__choice(mess, width, height, interval, default_font, freezePlayer, playerCloseable, choices, pos) : DialogBox
@__messageBox(mess, width, height, interval, default_font, freezePlayer, playerCloseable, pos) : DialogBox
@__createMessageBox(mess, width, height, interval, default_font, freezePlayer, playerCloseable, pos) : DialogBox
@__getDialog() : DialogBox
@__fade(time, delay, start)
@__vec(x : float = 0, y : float = 0) : Vector2
@__rectangle(x : int = 0, y : int = 0, width : int = 0, height : int = 0) : Rectangle
@__color(r : int = 0, g : int = 0, b : int = 0, a : int = 0) : Color
@getPlayerX() : float
@getPlayerY() : float
@random() : float

// Content related functions
@getAnim(obj) : any
@getSound(ident) : any
@getContent(ident) : any

// Input related functions
@isKeyDown(key) : bool
@isKeyBeingPressed(key) : bool
@isButtonDown(button) : bool
@isButtonBeingPressed(button) : bool
@getKeyInterval(key) : int
@getButtonInterval(button) : int

// Sound related functions
@playSound(sound, volume=1, pitch=0, pan=0)
@stopSound(sound)

// Animation related functions
@setAnim(obj)
@createGDAnim(anim)
@createGDImage(text)
@getAnimFrame(anim=null)
@getAnimFrameInt(anim=null)
@getAnimFrameCount(anim=null)
@setAnimFrame(anim, frame)
@pauseAnim(obj=null)
@resumeAnim(obj=null)

// Texture related functions
@textureFromFile(file)

// Reflection related functions
@getStatic(class : any, field : any) : any
@setStatic(class : any, field : any, value : any) : void
@getStaticProp(class : any, prop : any) : any
@setStaticProp(class : any, prop : any, value : any) : any
//@is(obj : any, type) : bool
//@new(class, args=null, types=null)
@hasField(obj : any, field) : bool
@hasProperty(obj : any, prop) : bool
@hasMethod(obj : any, method) : bool

// Misc methods
@sw() : System.Diagnostics.Stopwatch

// Shows a generic message on the screen
func showGenericMessage(mess, delay=2, font=null) : GenericMessage
{
    if(font == null)
        font = font_large;
    
    var genMess = new Zombie_House.Entities.UI.GenericMessage(mess, delay, font);
    
    level.HUD.AddChild(genMess, -1);
    
    return genMess;
}
// Shows a choice dialog
func choice(message : string, choices : [any], width=290, height=-1, interval=0, pos=-1, default_font=null) : DialogBox
{
    return __choice(message, width, height, interval, default_font, true, true, choices, pos);
}
// Shows a dialog message on the screen
func showMessage(message : string, width = 290, height = 90, interval=0, pos=-1, default_font=null) : DialogBox
{
    return __messageBox(message, width, height, interval, default_font, true, true, pos);
}
// Creates a message dialog box but doesn't display it on screen
// Useful when chaining multiple dialogs one after another
func createMessage(message : string, width = 290, height = 90, interval=0, pos=-1, default_font=null) : DialogBox
{
    return __createMessageBox(message, width, height, interval, default_font, true, true, pos);
}
// Gets the currently openned dialog
func getDialog : DialogBox
{
    return __getDialog();
}

// Fades the screen, optionally specifying timer, delay and starting timer parameters
func fadeScreen(time : int = 50, d : int = 20, start : int = -1) : void
{
    __fade(time, d, start);
}

// Pauses the current fade operation
func pauseFade : void
{
    level.PauseFade();
}
// Resumes the current fade operation
func resumeFade : void
{
    level.ResumeFade();
}

// Adds a new particle emitter to the level
func addEmitter(x : int = 0, y : int = 0, w : int = 0, h : int = 0, particleType : string = 'Smoke') : ParticleEmitter
{
    // Add the particle emitter for the black smoke
    var emitter = new Zombie_House.Entities.ParticleEmitter();

    emitter.CollisionRectangle = rec(x, y, w, h);
    emitter.BaseVelocity = vec(0, -1 / 32);
    emitter.RandomVelocity = vec(6, 0);
    emitter.BaseGravity = vec(0, -0.0125 / 4);
    emitter.BaseFriction = vec(0.7, 1);
    emitter.BaseScale = 0.5;
    emitter.RandomScale = 0.1;
    emitter.ParticleName = particleType;
    emitter.BaseFadeDelta = 0.0125 / 2;
    emitter.RandomFadeDelta = 0.0025 / 2;
    
    level.Root.addChild(emitter, -1);
    
    return emitter;
}

// Adds a light to the gamelevel
func addLight(x : float, y : float, range : float, _color : Color, intensity : float = 1, angle : float = 0, fov : float = 6.283185307179586) : Light2D
{
    return level.addLight(x, y, range, _color, intensity, angle, fov);
}

// Returns a Vector2 object with the given coordinates
func vec(x : float = 0, y : float = 0) : Vector2
{
    return __vec(x, y);
}
// Returns a Rectangle object with the given coordinates
func rec(x : int = 0, y : int = 0, w : int = 0, h : int = 0) : Rectangle
{
    return __rectangle(x, y, w, h);
}
// Returns a Color object with the given components
func color(r : int = 0, g : int = 0, b : int = 0, a : int = 0) : Color
{
    return __color(r, g, b, a);
}

// Draggalbe functions

// Adds a new draggable to the game level
func addDraggable(x : int, y : int, drag, mid = -1) : any
{
    level.addDraggable(x, y, drag, mid);
    
    return drag;
}

// Weather related functions
let weather_none = 'none';
let weather_rain = 'rain';

typeAlias WeatherEffect : "Zombie_House.Entities.Particles.WeatherEffect"
{
    
}

typeAlias RainEffect <- WeatherEffect : "Zombie_House.Entities.Particles.RainEffect"
{
    
}

// Sets the weather of the level
func setWeather(weatherType : string, weatherParams = null) : WeatherEffect
{
    // Create the new weather
    var newWeather : Zombie_House.Entities.Particles.WeatherEffect = null;
    
    if(weatherType == 'rain')
        newWeather = new RainEffect(weatherParams);
    else if(weatherType == 'none')
        newWeather = new WeatherEffect();
    
    // Finish the current weather
    level.CurrentWeather.End();

    level.CurrentWeather = newWeather;
    newWeather.Init(level);
    newWeather.LoadContent(level.MainEngine.Game.Content);
    
    return newWeather;
}

// Keyboard keycodes
let Key_None = 0;
let Key_Back = 8;
let Key_Tab = 9;
let Key_Enter = 13;
let Key_Pause = 19;
let Key_CapsLock = 20;
let Key_Kana = 21;
let Key_Kanji = 25;
let Key_Escape = 27;
let Key_ImeConvert = 28;
let Key_ImeNoConvert = 29;
let Key_Space = 32;
let Key_PageUp = 33;
let Key_PageDown = 34;
let Key_End = 35;
let Key_Home = 36;
let Key_Left = 37;
let Key_Up = 38;
let Key_Right = 39;
let Key_Down = 40;
let Key_Select = 41;
let Key_Print = 42;
let Key_Execute = 43;
let Key_PrintScreen = 44;
let Key_Insert = 45;
let Key_Delete = 46;
let Key_Help = 47;
let Key_D0 = 48;
let Key_D1 = 49;
let Key_D2 = 50;
let Key_D3 = 51;
let Key_D4 = 52;
let Key_D5 = 53;
let Key_D6 = 54;
let Key_D7 = 55;
let Key_D8 = 56;
let Key_D9 = 57;
let Key_A = 65;
let Key_B = 66;
let Key_C = 67;
let Key_D = 68;
let Key_E = 69;
let Key_F = 70;
let Key_G = 71;
let Key_H = 72;
let Key_I = 73;
let Key_J = 74;
let Key_K = 75;
let Key_L = 76;
let Key_M = 77;
let Key_N = 78;
let Key_O = 79;
let Key_P = 80;
let Key_Q = 81;
let Key_R = 82;
let Key_S = 83;
let Key_T = 84;
let Key_U = 85;
let Key_V = 86;
let Key_W = 87;
let Key_X = 88;
let Key_Y = 89;
let Key_Z = 90;
let Key_LeftWindows = 91;
let Key_RightWindows = 92;
let Key_Apps = 93;
let Key_Sleep = 95;
let Key_NumPad0 = 96;
let Key_NumPad1 = 97;
let Key_NumPad2 = 98;
let Key_NumPad3 = 99;
let Key_NumPad4 = 100;
let Key_NumPad5 = 101;
let Key_NumPad6 = 102;
let Key_NumPad7 = 103;
let Key_NumPad8 = 104;
let Key_NumPad9 = 105;
let Key_Multiply = 106;
let Key_Add = 107;
let Key_Separator = 108;
let Key_Subtract = 109;
let Key_Decimal = 110;
let Key_Divide = 111;
let Key_F1 = 112;
let Key_F2 = 113;
let Key_F3 = 114;
let Key_F4 = 115;
let Key_F5 = 116;
let Key_F6 = 117;
let Key_F7 = 118;
let Key_F8 = 119;
let Key_F9 = 120;
let Key_F10 = 121;
let Key_F11 = 122;
let Key_F12 = 123;
let Key_F13 = 124;
let Key_F14 = 125;
let Key_F15 = 126;
let Key_F16 = 127;
let Key_F17 = 128;
let Key_F18 = 129;
let Key_F19 = 130;
let Key_F20 = 131;
let Key_F21 = 132;
let Key_F22 = 133;
let Key_F23 = 134;
let Key_F24 = 135;
let Key_NumLock = 144;
let Key_Scroll = 145;
let Key_LeftShift = 160;
let Key_RightShift = 161;
let Key_LeftControl = 162;
let Key_RightControl = 163;
let Key_LeftAlt = 164;
let Key_RightAlt = 165;
let Key_BrowserBack = 166;
let Key_BrowserForward = 167;
let Key_BrowserRefresh = 168;
let Key_BrowserStop = 169;
let Key_BrowserSearch = 170;
let Key_BrowserFavorites = 171;
let Key_BrowserHome = 172;
let Key_VolumeMute = 173;
let Key_VolumeDown = 174;
let Key_VolumeUp = 175;
let Key_MediaNextTrack = 176;
let Key_MediaPreviousTrack = 177;
let Key_MediaStop = 178;
let Key_MediaPlayPause = 179;
let Key_LaunchMail = 180;
let Key_SelectMedia = 181;
let Key_LaunchApplication1 = 182;
let Key_LaunchApplication2 = 183;
let Key_OemSemicolon = 186;
let Key_OemPlus = 187;
let Key_OemComma = 188;
let Key_OemMinus = 189;
let Key_OemPeriod = 190;
let Key_OemQuestion = 191;
let Key_OemTilde = 192;
let Key_ChatPadGreen = 202;
let Key_ChatPadOrange = 203;
let Key_OemOpenBrackets = 219;
let Key_OemPipe = 220;
let Key_OemCloseBrackets = 221;
let Key_OemQuotes = 222;
let Key_Oem8 = 223;
let Key_OemBackslash = 226;
let Key_ProcessKey = 229;
let Key_OemCopy = 242;
let Key_OemAuto = 243;
let Key_OemEnlW = 244;
let Key_Attn = 246;
let Key_Crsel = 247;
let Key_Exsel = 248;
let Key_EraseEof = 249;
let Key_Play = 250;
let Key_Zoom = 251;
let Key_Pa1 = 253;
let Key_OemClear = 254;

// Button codes
let Button_DPadUp = 1;
let Button_DPadDown = 2;
let Button_DPadLeft = 4;
let Button_DPadRight = 8;
let Button_Start = 16;
let Button_Back = 32;
let Button_LeftStick = 64;
let Button_RightStick = 128;
let Button_LeftShoulder = 256;
let Button_RightShoulder = 512;
let Button_BigButton = 2048;
let Button_A = 4096;
let Button_B = 8192;
let Button_X = 16384;
let Button_Y = 32768;
let Button_LeftThumbstickLeft = 2097152;
let Button_RightTrigger = 4194304;
let Button_LeftTrigger = 8388608;
let Button_RightThumbstickUp = 16777216;
let Button_RightThumbstickDown = 33554432;
let Button_RightThumbstickRight = 67108864;
let Button_RightThumbstickLeft = 134217728;
let Button_LeftThumbstickUp = 268435456;
let Button_LeftThumbstickDown = 536870912;
let Button_LeftThumbstickRight = 1073741824;

#include common.zhs

sequence seq
[
    1
    {
        spawnEnemy(baseZombie, { x: player.x - 200, y : player.y, hp: 25 });
        spawnEnemy(baseZombie, { x: player.x - 100, y : player.y, hp: 50 });
        
        changePlayerWeapon(player, WEAPON_SHOTGUN, 0, 4);
        
        addInventoryItem(player, itemid_shotgun_shells, 15);
        
        setCinematicMode(true);
        
        facePlayer(null, -1);
    }
    
    50-300
    {
        if(canPlayerAttack())
            makePlayerAttack();
    }
    
    {
        makePlayerReload();
    }
    
    +50
    {
        facePlayer(null, 1);
    }
    
    +50
    {
        facePlayer(null, -1);
    }
    
    +100-220
    {
        makePlayerAttack();
    }
    
    {
    	setCinematicMode(false);
    }
]

object RunBehavior : ConstantBehavior
{
    var easingIn : float = 5;
    var easingOut : float = 5;
    
    func RunBehavior(_easingIn = 5, _easingOut = -1)
    {
        base();
        
        easingIn = _easingIn;
        if(_easingOut == -1)
            easingOut = _easingIn;
        else
            easingOut = _easingOut;
    }
    
    func step()
    {
        if(!player.codeControlled)
        {
            if(player.GetIntState() == HumanState_Standing && allowInput && (getKeyInterval(Key_NumPad5) > -1))
            {
                player.speedMod += (1.0 - player.speedMod) / easingIn;
            }
            else
            {
                player.speedMod += (0.65 - player.speedMod) / easingOut;
            }
        }
    }
}

object PlayerLightBehavior : ConstantBehavior
{
    var playerLight = null;
    var playerLight2 = null;
    var turnTimer : float = 0;
    var side : int = 1;
    var lightsOn : bool = true;
    
    func PlayerLightBehavior()
    {
        base();
        
        var lightColor : Color = color(255, 255, 255, 255);
        
        playerLight = addLight(player.X, player.Y, 180, lightColor, 0.6, 0, PI / 3);
        playerLight2 = addLight(player.X, player.Y, 90, lightColor, 0.6);
    }
    
    func step()
    {
        if(playerLight != null)
        {
            if(player.ScaleX != side)
            {
                turnTimer = 9;
                side = player.ScaleX;
            }
            
            playerLight.X = player.X;
            playerLight.Y = player.Y;
            
            if(player.crouching)
                playerLight.Y = player.Y + 5;
            
            // Perform a Raycast so we don't accidentally cross walls with the light while moving it over
            var v = level.RaycastPoint(vec(playerLight.X, playerLight.Y), vec(playerLight.X - ((turnTimer - 4.5) / 4) * player.ScaleX * 45, playerLight.Y), true);
            
            playerLight2.X = v.X;
            playerLight2.Y = playerLight.Y;
            
            playerLight.Angle = 0;
            if(player.ScaleX == -1)
                playerLight.Angle = -PI;
            
            if(hasInventoryItem(player, itemid_flashlight))
            {
                if(allowInput && getKeyInterval(Key_L) == 0)
                {
                    lightsOn = !lightsOn;
                }
            }
            else
            {
                lightsOn = false;
            }
            
            turnTimer--;
            
            if(turnTimer > 0)
            {
                playerLight.IsOn = false;
                playerLight2.IsOn = lightsOn;
            }
            else
            {
                playerLight.IsOn = lightsOn;
                playerLight2.IsOn = false;
            }
        }
    }
}

let baseHealth : int = 10;

let baseZombie = { type:ENEMY_BUILDINGZOMBIE, hp:baseHealth };

var elevatorDisplayLights : [any] = [ ];
var elevatorButtonLights : [any] = [ ];

var runBeh : RunBehavior = null;
var playerLightBeh : PlayerLightBehavior = null;

func init
{
    runBeh = RunBehavior(10, 3);
    playerLightBeh = PlayerLightBehavior();
    
    addInventoryItem(player, itemid_flashlight, 1, null, 24);
    
    setupLights();
    
    setElevatorFloor(1);
    
    level.TeleportPlayerToElevator(12);
    level.ChangeFloor(12);
    
    //player.InventoryAvailable = false;
    //player.CanAttack = false;
    //player.HUDRef.SetHudStyle(1);
    
    level.ReverseLightingDrawOrder = true;
    
    setElevatorFloor(1);
    
    changePlayerWeapon(player, WEAPON_SHOTGUN, 0, 4);
    
    addInventoryItem(player, itemid_shotgun_shells, 15);
    
    //startSequence('seq');
    //changePlayerWeapon(player, WEAPON_SHOTGUN, 0, 4);
    
    //addInventoryItem(player, itemid_shotgun_shells, 15);
    //zdead = spawnEnemy(baseZombie, { x: player.x - 200, y : player.y, hp: 10 });
    
    //setTimeout('setWeather', 1, [weather_rain, [vec(1, 3), vec(0, 0.1), toUint(0x53FFFFFF), 3.1]]);
    //startSequence('start');
    //changePlayerWeapon(player, WEAPON_PISTOL, 0);
    //changePlayerWeapon(player, WEAPON_SHOTGUN, 1);
    //addInventoryItem(player, itemid_pistol_magazine, 5);
    //addInventoryItem(player, itemid_shotgun_shells, 31);
    //player.FacingDirection = -1;
}
func zdead(z:Enemy=null)
{
    if(z.TotalHP <= 1)
        return;
    
    // zdead = spawnEnemy(baseZombie, { x: z.X, y : z.Y, hp: z.TotalHP / 2 });
}
func step(time : float) : void
{
    if(isKeyBeingPressed(Key_H))
    {
        player.ApplyEffect(new Zombie_House.Entities.Enemies.ZombieBiteSlowEffect(0.5));
    }
}
func end()
{
    
}

func setupLights()
{
    for(var i = 0; i < 13; i++)
    {
        elevatorDisplayLights.Add(addLight(273, 36 + 34 * i, 7, color(255, 255, 120, 255), 0.65));
        elevatorButtonLights.Add(addLight(268, 53 + 34 * i, 10, color(255, 255, 255, 255), 0.3));
    }
}

// Sets the elevator floor
func setElevatorFloor(_floor)
{
    level.ChangeElevatorFloor(_floor);
    
    for(var i = 0; i < elevatorDisplayLights.Count; i++)
    {
        elevatorDisplayLights[i].X = 273 + toInt((_floor / 21.0) * 17);
    }
}
// Gets the current elevator floor
func getElevatorFloor()
{
    level.CurrentElevatorFloor;
    
    setElevatorFloor(0);
}

func onInteractStart(inte)
{
    if(hasField(inte, 'Data'))
    {
        if(inte.Data == "floor6_4")
        {
            inte.StopInteraction();
            
            playSound("DoorLocked1");
            
            showMessage("The door is unlocked, but I can't open it because something's blocking it from the other side", 290, 90, 2).FollowupDialog = 
            createMessage("If I had a crowbar or something I could maybe unbudge by brute force.", 290, 90, 2);
        }
    }
}
func onInteractUpdate(inte) { }
func onInteractEnd(inte) { }

func onDragStart(drag) { }
func onDragUpdate(drag) { }
func onDragEnd(drag) { }

sequence start
[
    var elevatorDoor;
    var elevatorDoor2;
    var times = 0;
    var explosions = 0.0;
    
    0
    {
        async = false;
        pauseFade();

        player.InventoryAvailable = false;
        player.CanAttack = false;
        player.HUDRef.SetHudStyle(1);
        
        level.ReverseLightingDrawOrder = true;
        
        freezePlayer();
        hideHud();
        
        // Disable all the enemies temporarely while the script runs
        var enArray = level.Enemies;
        var enLength = enArray.Count;
        for(var i = 0; i < enLength; i++)
        {
            enArray[i].AIEnabled = false;
        }
        
        setElevatorFloor(10);
    }
    {
        freezePlayer();
        fadeScreen(200, 200, 100);
    }
    +70
    {
        startMovingPlayer(player, -1, -1, 0.75);
    }
    +50
    {
        startSequence('placingSequence');
    }
    +293
    {
        stopMovingPlayer(player, 1);
        player.animCodeControlled = true;
        changeAnim(player, getAnim("PlayerTickSwitch"));
    }
    +10
    {
        playSound("ElevatorButton");
        showGenericMessage("Hit <FIRE> to open the elevator door!", 5);
    }
    +1
    {
        elevatorDoor = createGDAnim(getAnim("Building_ElevatorDoorExplode"));
        
        elevatorDoor.X = 264;
        elevatorDoor.Y = 273;
        
        level.InteractiblesHolder.addChild(elevatorDoor, -1);
    }
    +2
    {
        if(getAnimFrameInt() == 8)
        {
            pauseAnim();
        }
        
        if(getKeyInterval(Key_NumPad1) < 2 && getKeyInterval(Key_NumPad1) != -1 && getAnimFrameInt() > 3 && explosions < 4)
        {
            setAnimFrame(player, 3);
            resumeAnim();
            playSound("ElevatorButton");
            times++;
            
            if(times % 8 == 0)
            {
                level.CameraShake = 1;
                explosions++;
                
                setAnimFrame(elevatorDoor, getAnimFrame(elevatorDoor) + 1);
                
                if(explosions == 4)
                {
                    playSound("BigExplosion");
                    level.CameraShake = 2;
                    
                    var emit = addEmitter(elevatorDoor.x, elevatorDoor.y, elevatorDoor.width, elevatorDoor.height);
                    emit.tint = color(0, 0, 0, 255);
                    emit.ParticlesPerEmit = 60;
                    emit.RandomScale = 1;
                    emit.Timeout = 2;
                    emit = null;
                }
                else
                {
                    playSound("SmallExplosion", 1, 0 - (explosions - 1) / 10);
                    
                    var emit = addEmitter(elevatorDoor.x, elevatorDoor.y, elevatorDoor.width, elevatorDoor.height);
                    emit.ParticlesPerEmit = 30;
                    emit.Timeout = 4;
                    emit = null;
                }
            }
        }
        
        if(explosions < 4)
        {
            this.Frame = T - 1;
        }
    }
    +1
    {
        if(getAnimFrame() != getAnimFrameCount() - 1)
        {
            this.Frame = T - 1;
        }
        else
        {
            player.animCodeControlled = false;
        }
    }
    +100
    {
        startMovingPlayer(player, 1, 1, 0.75);
    }
    +35
    {
        stopMovingPlayer();
    }
    +10
    {
        elevatorDoor2 = createGDAnim(getAnim("Building_ElevatorDoorOver"));
        
        elevatorDoor2.x = 272;
        elevatorDoor2.y = 276;
        
        setAnimFrame(elevatorDoor2, getAnimFrameCount(elevatorDoor2) - 1);
        
        level.Root.addChild(elevatorDoor2, -1);
    }
    +7
    {
        if(getAnimFrame(elevatorDoor2) != 0)
        {
            this.Frame = T - 4;
            setAnimFrame(elevatorDoor2, getAnimFrame(elevatorDoor2) - 1);
        }
    }
    {
        level.InteractiblesHolder.removeChild(elevatorDoor, false);
        level.Root.removeChild(elevatorDoor2, false);
        elevatorDoor.Free(false);
        elevatorDoor2.Free(false);
        
        player.visible = false;
    }
    {
        fadeScreen(70, 180);
        playSound("ElevatorRideFail");
    }
    +70
    {
        level.TeleportPlayerToElevator(12);
        level.ChangeFloor(12);
        player.visible = true;
        
        setWeather(weather_rain, [vec(1, 3), vec(0, 0.1), toUint(0x53FFFFFF), 0]);
        startSequence("makeRainStrong");
    }
    +110
    {
        playSound("ElevatorArrive");
    }
    +10
    {
        player.codeControlled = false;
        unfreezePlayer();
        showHud();
    }

/*
    +15
    {
        playSound("ElevatorButton");
    }
    +4
    {
        playSound("ElevatorRideFail");
    }
    +5
    {
        if(getAnimFrame() != getAnimFrameCount() - 1)
        {
            this.Frame = T - 1;
        }
        else
        {
            player.animCodeControlled = false;
            player.speedMod = 1;
        }
    }
    -5
    +30, +60, +90, +120
    {
        setElevatorFloor(getElevatorFloor() - 1);
    }
    +50
    {
        unfreezePlayer();
        showHud();
        
        // Re-enable all the enemies
        for(i = 0; i < enLength; i++)
        {
            enArray[i].AIEnabled = true;
        }
    }
*/
]

sequence makeRainStrong
[
    1-5000
    {
        var weather = level.CurrentWeather; weather.Strength = weather.Strength + 0.0002;
    }
]

// Sequence that displays the place where the story is being taken
sequence placingSequence
[
    var hour;
    var rest;
    var timeMsg;

    0
    {
        showGenericMessage("October 19th\nGenAttics Ltda. building\nCalifornia", framesToSeconds(200), font_default);
    }

    +200
    {
        hour = "8";
        rest = "53 PM";
        timeMsg = showGenericMessage(hour + ":" + rest, framesToSeconds(270), font_default);
    }
    +30, +90, +150, +210, +270
    {
        timeMsg.Text.Text = hour + "." + rest;
    }
    -270
    +1, +60, +120, +180, +240
    {
        timeMsg.Text.Text = hour + ":" + rest;
    }
    -240
    +119
    {
        rest = "54 PM";
    }
]

// Sequence that displays the credits for the game
sequence creditsSequence
[
    0
    {
        showGenericMessage("  A Game by\nLuiz Fernando", framesToSeconds(200));
    }

    +200
    {
        showGenericMessage("", framesToSeconds(200));
    }

    +200
    {
        showGenericMessage("NHANHANHA", framesToSeconds(200));
    }
]

#include weapon_commons.zhs

// Includes common player functions that can be used to control the player entities like actors and make them perform actions like
// move, attack, take damage, use items, die, etc.

// Starts moving the given player, optionally specifying the moving and facing direction and the moving speed
// This function automatically sets the Player.codeControlled field to true
func startMovingPlayer(_player : Player = null, movingDirection : int = -2, facingDirection : int = -2, speed : float = -1) : void
{
    if(_player == null)
        _player = player;
    
    if(movingDirection == -2)
        movingDirection = _player.MovingDirection;
    
    // By default, face the player on the same direction it is moving
    if(facingDirection == -2)
        facingDirection = movingDirection;
    
    _player.moving = true;
    _player.MovingDirection = movingDirection;
    _player.FacingDirection = facingDirection;
    
    if(speed != -1)
        _player.speedMod = speed;
    
    _player.codeControlled = true;
}

// Stops moving the given player, optionally making it face the given direction and specifying the speed modifier
func stopMovingPlayer(_player : Player = null, facingDirection : int = -2, speed : int = -1) : void
{
    if(_player == null)
        _player = player;

    _player.moving = false;

    if(facingDirection != -2)
        _player.FacingDirection = facingDirection;

    if(speed != -1)
        _player.speedMod = speed;
}
// Makes the given player entity face the given direction, optionally changing the moving direction as well
func facePlayer(_player : Player = null, facingDirection : int = 1, movingDirection : int = -2) : void
{
    if(_player == null)
        _player = player;

    if(movingDirection != -2)
        _player.MovingDirection = movingDirection;

    _player.FacingDirection = facingDirection;
}

// Returns whether the given player can attack currently
func canPlayerAttack(_player : Player = null) : bool
{
    if(_player == null)
        _player = player;
    
    return !_player.GetWeapon(-1).IsEmpty();
}

// Makes the given player entity attack with the current wapon, with an optional parameter that can be used to force the attack even when the player is not resting
func makePlayerAttack(_player : Player = null, force : bool = false) : void
{
    if(_player == null)
        _player = player;

    if(force || player.GetIntState() == 0)
        player.FireWeapon();
}
// Makes the given player entity reload the current wapon, with an optional parameter that can be used to force the reload operation even when the player is not resting
func makePlayerReload(_player : Player = null, force : bool = false) : void
{
    if(_player == null)
        _player = player;

    if(force || player.GetIntState() == 0)
        player.ReloadWeapon();
}

// Changes the animation of the player to be the given one
func changeAnim(_player : Player = null, newAnim : AnimationDescriptor = null, force : bool = false) : void
{
    if(_player == null)
        _player = player;

    player.ChangeAnim(_player.Body, newAnim, force);
}

// Freezes the given player instance, not allowing the player to interact with it, optionally specifying if any current interactions should be canceled
func freezePlayer(_player : Player = null, endInteract : bool = false) : void
{
    if(_player == null)
        _player = player;

    _player.Freeze(endInteract);
}
// Unfreezes the given player instance, allowing the player to interact with it again, optionally specifying if the controls should be rested and no key presses should be considered during the next frame
func unfreezePlayer(_player : Player = null, restControls : bool = false) : void
{
    if(_player == null)
        _player = player;

    _player.Unfreeze(restControls);
}

// Damages the given player with the given ammount of damage, optionally specifying a damage source and redness modifier
// If the player's health reaches 0 or lower after the damage is dealt, the player is killed
func damagePlayer(_player : Player, damageAmm : float, damageSource : int = -2, rednessModifier : int = -1) : void
{
    _player.Damage(damageAmm, damageSource, rednessModifier);
}
// Heals the given player with the given heal ammount, optionally specifying a heal source
func healPlayer(_player : Player, healAmm : float, healSource : int =-2) : void
{
    _player.Heal(healAmm, healSource);
}
// Kills the given player, optionally specifying a kill source
func killPlayer(_player : Player = null, killSource : int = -2) : void
{
    if(_player == null)
        _player = player;

    _player.Die(killSource);
}

// Adds an inventory item to the given player's inventory
func addInventoryItem(_player : Player = null, itemID : int = 0, count : int = 1, hash : int = null, slot : int = 0) : void
{
    if(_player == null)
        _player = player;

    if(hash == null)
    {
        hash = { }.ToHashtable();
    }
    //if(is(hash, 'ZombieHouseScripter.ObjectDef'))
    
    if(hash is ZombieHouseScripter.Elements.ObjectDef)
    {              
        hash = hash.ToHashtable();
    }

    _player.CurrentInventory.AddItem(itemID, count, hash, slot);
}

// Removes an inventory item from the given player's inventory
func removeInventoryItem(_player : Player = null, itemID : int = 0, count : int = 1, slot : int = 0) : void
{
    if(_player == null)
        _player = player;
    
    _player.CurrentInventory.RemoveItem(itemID, count, slot);
}

// Returns the count of the specified item on the given player's inventory
func inventoryItemCount(_player : Player = null, itemID : int = 0) : any
{
    if(_player == null)
        _player = player;
    
    return _player.CurrentInventory.GetItemNum(itemID);
}

func hasInventoryItem(_player : Player = null, itemID : int = 0) : any
{
    return inventoryItemCount(_player, itemID) > 0;
}

// Hides the HUD of the given player ID. Leaving empty of passing a null player ID makes the function automatically assume the current main player character
func hideHud(_player : Player = null) : void
{
    if(_player == null)
        _player = player;

    player.HUDRef.Hide();
}
// Shows the HUD of the given player ID. Leaving empty of passing a null player ID makes the function automatically assume the current main player character
func showHud(_player : Player = null) : void
{
    if(_player == null)
        _player = player;

    player.HUDRef.Show();
}

func setCinematicMode(mode : bool = true) : void
{
    if(mode)
    {
        player.codeControlled = true;
        hideHud();
    }
    else
    {
        player.codeControlled = false;
        showHud();
    }
}

// Inventory Statics
let itemid_none : int = getStatic('Zombie_House.Entities.InventoryItem', 'ITEM_NONE');
let itemid_baseballbat : int = getStatic('Zombie_House.Entities.InventoryItem', 'ITEM_BASEBALLBAT');
let itemid_pistol : int = getStatic('Zombie_House.Entities.InventoryItem', 'ITEM_PISTOL');
let itemid_uzi : int = getStatic('Zombie_House.Entities.InventoryItem', 'ITEM_UZI');
let itemid_shotgun : int = getStatic('Zombie_House.Entities.InventoryItem', 'ITEM_SHOTGUN');
let itemid_m16 : int = getStatic('Zombie_House.Entities.InventoryItem', 'ITEM_M16');
let itemid_glauncher : int = getStatic('Zombie_House.Entities.InventoryItem', 'ITEM_GLAUNCHER');
let itemid_sawedoff_shotgun : int = getStatic('Zombie_House.Entities.InventoryItem', 'ITEM_SAWEDOFF_SHOTGUN');

let itemid_revolver : int = getStatic('Zombie_House.Entities.InventoryItem', 'ITEM_REVOLVER');
let itemid_revolver_ammo : int = getStatic('Zombie_House.Entities.InventoryItem', 'ITEM_REVOLVER_AMMO');
let itemid_pistol_magazine : int = getStatic('Zombie_House.Entities.InventoryItem', 'ITEM_PISTOL_MAGAZINE');
let itemid_shotgun_shells : int = getStatic('Zombie_House.Entities.InventoryItem', 'ITEM_SHOTGUN_SHELLS');
let itemid_uzi_magazine : int = getStatic('Zombie_House.Entities.InventoryItem', 'ITEM_UZI_MAGAZINE');
let itemid_m16_magazine : int = getStatic('Zombie_House.Entities.InventoryItem', 'ITEM_M16_MAGAZINE');

let itemid_grenade_round : int = getStatic('Zombie_House.Entities.InventoryItem', 'ITEM_GRENADE_ROUND');

let itemid_hand_grenade : int = getStatic('Zombie_House.Entities.InventoryItem', 'ITEM_HAND_GRENADE');

let itemid_empty_shell : int = getStatic('Zombie_House.Entities.InventoryItem', 'ITEM_EMPTY_SHELL');
let itemid_empty_round : int = getStatic('Zombie_House.Entities.InventoryItem', 'ITEM_EMPTY_ROUND');
let itemid_empty_rev_bullet : int = getStatic('Zombie_House.Entities.InventoryItem', 'ITEM_EMPTY_REV_BULLET');
let itemid_rivets : int = getStatic('Zombie_House.Entities.InventoryItem', 'ITEM_RIVETS');
let itemid_powder : int = getStatic('Zombie_House.Entities.InventoryItem', 'ITEM_POWDER');

let itemid_medkit : int = getStatic('Zombie_House.Entities.InventoryItem', 'ITEM_MEDKIT');
let itemid_pills : int = getStatic('Zombie_House.Entities.InventoryItem', 'ITEM_PILLS');

let itemid_flashlight : int = getStatic('Zombie_House.Entities.InventoryItem', 'ITEM_FLASHLIGHT');
let itemid_floor9_key : int = getStatic('Zombie_House.Entities.InventoryItem', 'ITEM_FLOOR9_KEY');

// States
let HumanState_Standing = 0;
let HumanState_Firing = 1;
let HumanState_Reloading = 2;
let HumanState_Interacting = 3;
let HumanState_Dragging = 4;
let HumanState_Stuck = 5;
let HumanState_Dead = 6;

typeAlias Weapon : "Zombie_House.Entities.Weapon"
{
    var a:int;
    func f() : int;
}

// Common weapon functions

// Changes the weapon the given player entity is carrying, optionally allowing a different slot to be selected
func changePlayerWeapon(_player, newWeapon, slot=-1, clip=-1) : Weapon
{
    var weapon = new Weapon(_player, newWeapon, clip, -1);
    _player.ChangeWeapon(weapon, slot);
    
    return weapon;
    
    /*var ammoID = getWeaponAmmoInventoryType(newWeapon);
    
    if(hasInventoryItem(_player, newWeapon, ammoID))
    {
        addInventoryItem(_player, ammoID)
    }*/
}

// Returns an identifier for the inventory item that serves as ammunition for the given weapon type
func getWeaponAmmoInventoryType(weaponID) : int
{
    switch(weaponID)
    {
        case WEAPON_PISTOL:
            return itemid_pistol_magazine;
        break;
        case WEAPON_UZI:
            return itemid_uzi_magazine;
        break;
        case WEAPON_SHOTGUN:
        case WEAPON_SAWNOFF:
            return itemid_shotgun_shells;
        break;
        case WEAPON_M16:
            return itemid_m16_magazine;
        break;
        case WEAPON_GL:
            return itemid_grenade_round;
        break;
        
        default:
            return 0;
        break;
    }
}

// Constants
let WEAPON_NONE = -2;
let WEAPON_FISTS = -1;
let WEAPON_BAT = 0;
let WEAPON_PISTOL = 1;
let WEAPON_UZI = 2;
let WEAPON_SHOTGUN = 3;
let WEAPON_M16 = 4;
let WEAPON_GL = 5;
let WEAPON_MINIGUN = 6;
let WEAPON_SAWNOFF = 7;


@__trace(a...)

func funca()
{
    var a = (i:int):(->) =>
            {
                if(i == 0)
                    return ()=>{ __trace(0); };
                else
                    return ()=>{ __trace(1); };
            };
    
    a(0)();
    a(1)();
    
    var c = o();
    var b = c.closure();
    b(10);
    __trace(c.c);
}

object o
{
    var c = 0;
    func  closure() : (int->)
    {
        return (i:int) => { c = i; };
    }
}
func funcb()
{
    var g1 = generator(1);
    var g2 = generator(3);
    __trace(g1());
    __trace(g2());
    __trace(g1());
    __trace(g2());
    __trace(g1());
    __trace(g2());
}
func generator(start:int) : (->int)
{
    return ():int => { return start++; };
}