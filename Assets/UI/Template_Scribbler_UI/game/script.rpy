# The script of the game goes in this file.

# Declare characters used by this game. The color argument colorizes the
# name of the character.

define e = Character("Eileen")
define k = Character("Kibby")


# The game starts here.

label start:

    # Show a background. This uses a placeholder by default, but you can
    # add a file (named either "bg room.png" or "bg room.jpg") to the
    # images directory to show it.

    scene bg sky

    # This shows a character sprite. A placeholder is used, but you can
    # replace it by adding a file named "eileen happy.png" to the images
    # directory.

    # show eileen happy
    show kibby at truecenter

    # These display lines of dialogue.

    k "Did you change the name and save directory of the game in options.rpy?"

    $ answer = renpy.input("Did you change the values at the top of options.rpy?").strip().lower()

    if answer == "yes":
        k "Good job"
    else:
        k "If not, you should do so right away! Saves will not work properly until you do."

    $ renpy.notify("This is a notification")

    menu:
        "This is a sample choice menu"
        "Choice 1":
            pass
        "Choice 2":
            pass
    # This ends the game.

    return
