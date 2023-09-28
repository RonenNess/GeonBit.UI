#region File Description
//-----------------------------------------------------------------------------
// Generate message boxes and other prompts.
//
// Author: Ronen Ness.
// Since: 2017.
//-----------------------------------------------------------------------------
#endregion
using GeonBit.UI.Entities;
using GeonBit.UI.Entities.TextValidators;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.IO;

namespace GeonBit.UI.Utils
{
    /// <summary>
    /// GeonBit.UI.Utils contain different utilities and helper classes to use GeonBit.UI.
    /// </summary>
    [System.Runtime.CompilerServices.CompilerGenerated]
    class NamespaceDoc
    {
    }

    /// <summary>
    /// Helper class to generate message boxes and prompts.
    /// </summary>
    public static class MessageBox
    {
        /// <summary>
        /// Return object containing all the data of a message box instance.
        /// </summary>
        public class MessageBoxHandle
        {
            /// <summary>
            /// Message box panel.
            /// </summary>
            public Panel Panel;

            /// <summary>
            /// Object used to fade out the background.
            /// </summary>
            public Entity BackgroundFader;

            /// <summary>
            /// Message box bottom buttons.
            /// </summary>
            public Button[] Buttons;

            /// <summary>
            /// Hide / close the message box.
            /// </summary>
            public void Close()
            {
                if (Panel.Parent != null)
                {
                    Panel.RemoveFromParent();
                    if (BackgroundFader != null) { BackgroundFader.RemoveFromParent(); }
                    OpenedMsgBoxesCount--;
                }
            }
        }

        /// <summary>
        /// Default size to use for message boxes.
        /// </summary>
        public static Vector2 DefaultMsgBoxSize = new Vector2(480, -1);

        /// <summary>
        /// Default text for OK button.
        /// </summary>
        public static string DefaultOkButtonText = "OK";

        /// <summary>
        /// Will block and fade background with this color while messages are opened.
        /// </summary>
        public static Color BackgroundFaderColor = new Color(0, 0, 0, 100);

        /// <summary>
        /// Count currently opened message boxes.
        /// </summary>
        public static int OpenedMsgBoxesCount
        {
            get; private set;
        } = 0;

        /// <summary>
        /// Get if there's a message box currently opened.
        /// </summary>
        public static bool IsMsgBoxOpened
        {
            get { return OpenedMsgBoxesCount > 0; }
        }

        /// <summary>
        /// A button / option for a message box.
        /// </summary>
        public class MsgBoxOption
        {
            /// <summary>
            /// Option title (for the button).
            /// </summary>
            public string Title;

            /// <summary>
            /// Callback to run when clicked. Return false to leave message box opened (true will close it).
            /// </summary>
            public Func<bool> Callback;

            /// <summary>
            /// Create the message box option.
            /// </summary>
            /// <param name="title">Text to write on the button.</param>
            /// <param name="callback">Action when clicked. Return false if you want to abort and leave the message opened, return true to close it.</param>
            public MsgBoxOption(string title, Func<bool> callback)
            {
                Title = title;
                Callback = callback;
            }
        }

        /// <summary>
        /// Show a message box with yes/no options.
        /// </summary>
        /// <param name="header">Messagebox header.</param>
        /// <param name="text">Main text.</param>
        /// <param name="onYes">Callback to invoke when clicking yes. Return true to close the messagebox when done.</param>
        /// <param name="onNo">Callback to invoke when clicking no. Return true to close the messagebox when done.</param>
        /// <param name="yesText">Text to use for the 'yes' button.</param>
        /// <param name="noText">Text to use for the 'no' button.</param>
        /// <returns>Message box handle.</returns>
        public static MessageBoxHandle ShowYesNoMsgBox(string header, string text, System.Func<bool> onYes, System.Func<bool> onNo, string yesText = "Yes", string noText = "No")
        {
            return ShowMsgBox(header, text, 
                new MsgBoxOption[] {
                    new MsgBoxOption(yesText, onYes != null ? onYes : () => {return true; }),
                    new MsgBoxOption(noText, onNo != null ? onNo: () => {return true; }),
                });
        }

        /// <summary>
        /// Show a message box with custom buttons and callbacks.
        /// </summary>
        /// <param name="header">Messagebox header.</param>
        /// <param name="text">Main text.</param>
        /// <param name="options">Msgbox response options.</param>
        /// <param name="extraEntities">Optional array of entities to add to msg box under the text and above the buttons.</param>
        /// <param name="size">Alternative size to use.</param>
        /// <param name="onDone">Optional callback to call when this msgbox closes.</param>
        /// <param name="parent">Parent to add message box to (if not defined will use root)</param>
        /// <returns>Message box handle.</returns>
        public static MessageBoxHandle ShowMsgBox(string header, string text, MsgBoxOption[] options, Entities.Entity[] extraEntities = null, Vector2? size = null, System.Action onDone = null, Entities.Entity parent = null)
        {
            // object to return
            MessageBoxHandle ret = new MessageBoxHandle();

            // create panel for messagebox
            size = size ?? new Vector2(500, -1);
            var panel = new Panel(size.Value);
            ret.Panel = panel;
            panel.AddChild(new Header(header));
            panel.AddChild(new HorizontalLine());
            panel.AddChild(new RichParagraph(text));

            // add to opened boxes counter
            OpenedMsgBoxesCount++;

            // add rectangle to hide and lock background
            ColoredRectangle fader = null;
            if (BackgroundFaderColor.A != 0)
            {
                fader = new ColoredRectangle(Vector2.Zero, Entities.Anchor.Center);
                fader.FillColor = new Color(0, 0, 0, 100);
                fader.OutlineWidth = 0;
                fader.ClickThrough = false;
                UserInterface.Active.AddEntity(fader);
                ret.BackgroundFader = fader;
            }

            // add custom appended entities
            if (extraEntities != null)
            {
                foreach (var entity in extraEntities)
                {
                    panel.AddChild(entity);
                }
            }

            // add bottom buttons panel
            var buttonsPanel = new Panel(new Vector2(0, 60), 
                PanelSkin.None, size.Value.Y == -1 ? Anchor.Auto : Anchor.BottomCenter);
            buttonsPanel.Padding = Vector2.Zero;
            panel.AddChild(buttonsPanel);
            buttonsPanel.PriorityBonus = -10;

            // add all option buttons
            var buttonsList = new List<Button>();
            var btnSize = new Vector2(options.Length == 1 ? 0f : (1f / options.Length), 60);
            foreach (var option in options)
            {
                // add button entity
                var button = new Button(option.Title, anchor: Anchor.AutoInline, size: btnSize);
                button.Identifier = option.Title;

                // set click event
                button.OnClick += (Entity ent) =>
                {
                    // if need to close message box after clicking this button, close it:
                    if (option.Callback == null || option.Callback())
                    {
                        // remove fader and msg box panel
                        if (fader != null) { fader.RemoveFromParent(); }
                        panel.RemoveFromParent();

                        // decrease msg boxes count
                        OpenedMsgBoxesCount--;

                        // call on-done callback
                        onDone?.Invoke();
                    }
                };

                // add button to buttons panel
                buttonsList.Add(button);
                buttonsPanel.AddChild(button);
            }
            ret.Buttons = buttonsList.ToArray();

            // add panel to given parent
            if (parent != null)
            {
                parent.AddChild(panel);
            }
            // add panel to active ui root
            else
            {
                UserInterface.Active.AddEntity(panel);
            }
            return ret;
        }

        /// <summary>
        /// Show a message box with just "OK".
        /// </summary>
        /// <param name="header">Message box title.</param>
        /// <param name="text">Main text to write on the message box.</param>
        /// <param name="closeButtonTxt">Text for the closing button (if not provided will use default).</param>
        /// <param name="size">Message box size (if not provided will use default).</param>
        /// <param name="extraEntities">Optional array of entities to add to msg box under the text and above the buttons.</param>
        /// <param name="onDone">Optional callback to call when this msgbox closes.</param>
        /// <returns>Message box handle.</returns>
        public static MessageBoxHandle ShowMsgBox(string header, string text, string closeButtonTxt = null, Vector2? size = null, Entity[] extraEntities = null, Action onDone = null)
        {
            return ShowMsgBox(header, text, new MsgBoxOption[]
            {
                new MsgBoxOption(closeButtonTxt ?? DefaultOkButtonText, null)
            }, size: size ?? DefaultMsgBoxSize, extraEntities: extraEntities, onDone: onDone);
        }

        /// <summary>
        /// Open a dialog to select file for saving.
        /// </summary>
        /// <param name="path">Path to start dialog in.</param>
        /// <param name="onSelected">Callback to trigger when a file was selected. Return true to close dialog, false to keep it opened.</param>
        /// <param name="onCancel">Callback to trigger when the user hit cancel.</param>
        /// <param name="options">File dialog flags.</param>
        /// <param name="filterFiles">Optional method to filter file names. Return false to hide files.</param>
        /// <param name="filterFolders">Optional method to filter folder names. Return false to hide folders.</param>
        /// <param name="title">File dialog title.</param>
        /// <param name="message">Optional message to show above files.</param>
        /// <param name="saveButtonTxt">String to show on the save file button.</param>
        /// <param name="cancelButtonTxt">String to show on the cancel button.</param>
        /// <param name="overrideWarning">If not null, will show this warning in a Yes/No prompt if the user tries to select an existing file.</param>
        /// <returns>Message box handle.</returns>
        public static MessageBoxHandle OpenSaveFileDialog(string path, Func<FileDialogResponse, bool> onSelected, Action onCancel = null!, FileDialogOptions options = FileDialogOptions.Default, Func<string, bool> filterFiles = null!, Func<string, bool> filterFolders = null!, string title = "Save File As..", string message = null!, string saveButtonTxt = "Save File", string cancelButtonTxt = "Cancel", string overrideWarning = "File '<filename>' already exists!\nAre you sure you want to override it?")
        {
            var currPath = string.IsNullOrEmpty(path) ? Path.GetFullPath(Directory.GetCurrentDirectory()) : Path.GetFullPath(path);

            // panel to contain file dialog
            var internalPanel = new Panel(new Vector2(0, 0), PanelSkin.None, Anchor.Auto);
            internalPanel.Padding = Vector2.Zero;
            internalPanel.AdjustHeightAutomatically = true;

            // show files and folders
            var filesList = new SelectList();
            filesList.Size = new Vector2(0, 364);
            internalPanel.AddChild(filesList);

            // create starting files list
            foreach (var file in Directory.GetFiles(currPath))
            {
                filesList.AddItem(Path.GetFileName(file));
            }

            // file name input
            var filenameInput = new TextInput(false);
            filenameInput.PlaceholderText = "Filename";
            filenameInput.Validators.Add(new FilenameValidator(true));
            filesList.OnValueChange = (Entity entity) => { if (filesList.SelectedValue != null) { filenameInput.Value = filesList.SelectedValue; } };
            internalPanel.AddChild(filenameInput);

            // create buttons: save
            List<MsgBoxOption> buttons = new List<MsgBoxOption>();
            buttons.Add(new MsgBoxOption(saveButtonTxt, () =>
            {
                var selectedPath = Path.Combine(currPath, filenameInput.Value);
                var fullPath = Path.GetFullPath(selectedPath);
                var close = onSelected?.Invoke(new FileDialogResponse()
                {
                    FullPath = fullPath,
                    RelativePath = selectedPath,
                    FileExists = File.Exists(fullPath)
                }) ?? true;
                return close;
            }));

            // create buttons: cancel
            if (options.HasFlag(FileDialogOptions.ShowCancelButton))
            {
                buttons.Add(new MsgBoxOption(cancelButtonTxt, () =>
                {
                    onCancel?.Invoke();
                    return true;
                }));
            }

            // show message box
            var handle = ShowMsgBox(title, message ?? string.Empty, buttons.ToArray(), new Entity[] { internalPanel }, size: new Vector2(700, 680));

            // make the save button disabled when no file is selected
            var saveBtn = handle.Buttons[0];
            saveBtn.BeforeDraw = (Entity entity) =>
            {
                saveBtn.Enabled = !string.IsNullOrEmpty(filenameInput.Value);
                if (saveBtn.Enabled)
                {
                    var selectedPath = Path.Combine(currPath, filenameInput.Value);
                    var fullPath = Path.GetFullPath(selectedPath);
                    if (!options.HasFlag(FileDialogOptions.AllowOverride) && File.Exists(fullPath))
                    {
                        saveBtn.Enabled = false;
                    }
                }
            };

            // return the handle
            return handle;
        }
    }

    /// <summary>
    /// Response we get from a file dialog message box.
    /// </summary>
    public struct FileDialogResponse
    {
        /// <summary>
        /// File path under root.
        /// </summary>
        public string RelativePath;

        /// <summary>
        /// Full file path.
        /// </summary>
        public string FullPath;

        /// <summary>
        /// If true, the selected file exists.
        /// </summary>
        public bool FileExists;
    }

    /// <summary>
    /// Flags for file dialogs.
    /// </summary>
    [Flags]
    public enum FileDialogOptions
    {
        /// <summary>
        /// File dialog will display folders and allow entering and leaving folders.
        /// </summary>
        AllowEnterFolders = 1 << 0,

        /// <summary>
        /// File dialog will not allow the user to leave the starting path. 
        /// </summary>
        CageInStartingPath = 1 << 1,

        /// <summary>
        /// Allow picking existing files in save file dialog.
        /// </summary>
        AllowOverride = 1 << 2,

        /// <summary>
        /// If true, will add a 'cancel' button to close dialog without selection.
        /// </summary>
        ShowCancelButton = 1 << 3,

        /// <summary>
        /// Default file dialog options.
        /// </summary>
        Default = AllowEnterFolders | AllowOverride | ShowCancelButton
    }
}
