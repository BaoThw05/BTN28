################################################################################
## Initialization
################################################################################

## The init offset statement causes the initialization statements in this file
## to run before init statements in any other file.
init offset = -2

## Calling gui.init resets the styles to sensible default values, and sets the
## width and height of the game.
init python:
    gui.init(1920, 1080)

define config.check_conflicting_properties = True

################################################################################
## GUI Configuration Variables
################################################################################
## Some choice gui values have been left in, to make them
## easier to adjust for accessibility purposes e.g. to allow
## players to change the default text font or size by rebuilding the gui.
## You may add more back if you need to adjust them, or find-and-replace
## any instances where they are used directly with their value.

# The text font for dialogue and choice menus
define gui.text_font = gui.preference("font", "MonaspaceNeon-Regular.otf")
# The text font for buttons
define gui.interface_text_font = gui.preference("interface_font", "MonaspaceNeon-Regular.otf")
# The default size of in-game text
define gui.text_size = gui.preference("size", 33)
# The font for character names
define gui.name_text_font = gui.preference("name_font", "MonaspaceNeon-Regular.otf")
# The size for character names
define gui.name_text_size = gui.preference("name_size", 45)

## Localization ################################################################

## This controls where a line break is permitted. The default is suitable
## for most languages. A list of available values can be found at
## https://www.renpy.org/doc/html/style_properties.html#style-property-language

define gui.language = "unicode"


################################################################################
## Style Initialization
################################################################################

init offset = -1

################################################################################
## Styles
################################################################################

style default:
    font gui.text_font
    size gui.text_size
    language gui.language

style input:
    adjust_spacing False

style hyperlink_text:
    font "fonts/MonaspaceNeon-Regular.otf"
    hover_underline True
    hover_color '#00B1E1'
    idle_color "#00719A"

style gui_text:
    color '#00B1E1'
    size gui.text_size
    font gui.interface_text_font

style button:
    xysize (None, None)
    padding (0, 0)
    hover_sound "audio/rollover3.ogg"
    activate_sound "audio/click.ogg"

style button_text:
    is gui_text
    yalign 0.5
    xalign 0.0
    font "MonaspaceNeon-Regular.otf"
    selected_color '#D26143'
    idle_color "#00B1E1"
    insensitive_color "#778288"
    hover_color "#E99067"
    size 30
    yoffset 6

style label_text:
    is gui_text
    size 36
    color '#D26143'


style bar:
    ysize 38
    left_bar Frame("gui/bar/left.png", 6, 6, 6, 6, tile=False)
    right_bar Frame("gui/bar/right.png", 6, 6, 6, 6, tile=False)

style vbar:
    xsize 38
    top_bar Frame("gui/bar/top.png", 6, 6, 6, 6, tile=False)
    bottom_bar Frame("gui/bar/bottom.png", 6, 6, 6, 6, tile=False)

style scrollbar:
    ysize 18
    base_bar Frame("gui/scrollbar/horizontal_[prefix_]bar.png", 6, 6, 6, 6, tile=False)
    thumb Frame("gui/scrollbar/horizontal_[prefix_]thumb.png", 6, 6, 6, 6, tile=False)
    unscrollable 'hide'

style vscrollbar:
    xsize 29
    base_bar Frame("gui/scrollbar/vertical_[prefix_]bar.png", 6, 6, 6, 6, tile=False)
    thumb "gui/scrollbar/vertical_[prefix_]thumb.png"
    unscrollable 'hide'
    
    thumb_offset 15
    left_gutter 10
    right_gutter 10

style slider:
    ysize 38
    base_bar Frame("gui/slider/horizontal_[prefix_]bar.png", 6, 6, 6, 6, tile=False)
    left_bar Frame("gui/slider/horizontal_[prefix_]bar_full.png", 6, 6, 6, 6, tile=False)
    right_bar Frame("gui/slider/horizontal_[prefix_]bar_empty.png", 6, 6, 6, 6, tile=False)
    thumb "gui/slider/horizontal_[prefix_]thumb.png"
    thumb_offset 20
    left_gutter 20
    right_gutter 20

style vslider:
    xsize 38
    base_bar Frame("gui/slider/vertical_[prefix_]bar.png", 6, 6, 6, 6, tile=False)
    thumb "gui/slider/vertical_[prefix_]thumb.png"


style frame:
    background Frame("gui/frame.png", 0, 36, 0, 19)
    padding (16, 33, 21, 46)
