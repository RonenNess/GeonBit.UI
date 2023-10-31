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
        public static MessageBoxHandle ShowMsgBox(string header, string text, MsgBoxOption[] options, Entity[] extraEntities = null, Vector2? size = null, Action onDone = null, Entity parent = null)
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
                button.AttachedData = ret;

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
            // current path
            var currPath = string.IsNullOrEmpty(path) ? Path.GetFullPath(Directory.GetCurrentDirectory()) : Path.GetFullPath(path);
            var originalPath = currPath;

            // add paragraph to show full path
            var fullPathLabel = new Label("");
            fullPathLabel.UseActualSizeForCollision = true;
            if (!options.HasFlag(FileDialogOptions.ShowDirectoryPath))
            {
                fullPathLabel.Visible = false;
            }
            
            // show files and folders
            var filesList = new SelectList();
            filesList.Size = new Vector2(0, 364);

            // file name input
            var filenameInput = new TextInput(false, Anchor.Auto);
            filenameInput.PlaceholderText = "Filename";
            filenameInput.Validators.Add(new FilenameValidator(true));
            filenameInput.Offset = new Vector2(0, -5);

            // if we must pick existing files, hide the file name input
            if (options.HasFlag(FileDialogOptions.MustSelectExistingFile))
            {
                filenameInput.Visible = false;
            }

            // create starting files list
            void UpdateFilesList()
            {
                // check for caging
                var cagePath = options.HasFlag(FileDialogOptions.CageInStartingPath);
                if (cagePath) 
                {
                    if (Path.GetFullPath(currPath).Length < Path.GetFullPath(originalPath).Length)
                    {
                        currPath = originalPath;
                    }
                }

                // update full path label
                fullPathLabel.Text = Path.GetFullPath(currPath);

                // if we must select existing file, reset selection when change folder
                if (options.HasFlag(FileDialogOptions.MustSelectExistingFile))
                {
                    filenameInput.Value = string.Empty;
                }

                // clear previous list
                filesList.ClearItems();

                // update size
                if (fullPathLabel.Visible)
                {
                    filesList.Size = new Vector2(filesList.Size.X, 380 - fullPathLabel.GetActualDestRect().Height);
                }

                // add folders
                if (options.HasFlag(FileDialogOptions.AllowEnterFolders) || options.HasFlag(FileDialogOptions.CanPickFolders))
                {
                    // add folder up
                    if (!cagePath || (Path.GetFullPath(currPath) != Path.GetFullPath(originalPath)))
                    {
                        filesList.AddItem("..");
                    }

                    // add folders
                    foreach (var dir in Directory.GetDirectories(currPath))
                    {
                        if (filterFolders == null || filterFolders(dir))
                        {
                            filesList.AddItem(Path.GetFileName(dir));
                            filesList.SetIcon("textures/folder_icon", filesList.Count - 1);
                        }
                    }
                }

                // add files
                foreach (var file in Directory.GetFiles(currPath))
                {
                    if (filterFiles == null || filterFiles(file))
                    {
                        filesList.AddItem(Path.GetFileName(file));
                        filesList.SetIcon("textures/file_icon", filesList.Count - 1);
                    }
                }
            }

            // click on files list - check if enter or exit folder
            if (options.HasFlag(FileDialogOptions.AllowEnterFolders))
            {
                filesList.OnSameValueSelected = (Entity entity) =>
                {
                    // on second click enter folder
                    if (filesList.SelectedValue != null)
                    {
                        // previous path to check if changed path
                        var prevPath = currPath;

                        // go one folder up
                        if (filesList.SelectedValue == "..")
                        {
                            var up = Directory.GetParent(currPath);
                            currPath = up != null ? up.ToString() : currPath;
                        }
                        // go into folder
                        else
                        {
                            if (Directory.Exists(Path.Combine(currPath, filesList.SelectedValue)))
                            {
                                currPath = Path.Combine(currPath, filesList.SelectedValue);
                            }
                        }

                        // update list if chaned path
                        if (prevPath != currPath)
                        {
                            UpdateFilesList();
                        }
                    }
                };
            }

            // on value change - set input name
            filesList.OnValueChange = (Entity entity) => 
            { 
                // set selected file name
                if (filesList.SelectedValue != null && filesList.SelectedValue != "..") 
                { 
                    filenameInput.Value = filesList.SelectedValue; 
                }
            };

            // return relative and full selected path
            (string, string) GetSelectedAndFullPath()
            {
                var selectedPath = Path.Combine(currPath, filenameInput.Value);
                var fullPath = Path.GetFullPath(selectedPath);
                return (selectedPath, fullPath);
            }

            // create buttons: save
            List<MsgBoxOption> buttons = new List<MsgBoxOption>();
            buttons.Add(new MsgBoxOption(saveButtonTxt, () =>
            {
                var paths = GetSelectedAndFullPath();
                var exists = File.Exists(paths.Item2);
                if (overrideWarning != null && exists)
                {
                    ShowYesNoMsgBox("Override File?", overrideWarning.Replace("<filename>", paths.Item2), () =>
                    {
                        var close = onSelected?.Invoke(new FileDialogResponse()
                        {
                            FullPath = paths.Item2,
                            RelativePath = paths.Item1,
                            FileExists = File.Exists(paths.Item2)
                        }) ?? true;
                        if (close)
                        {
                            (filesList.AttachedData as MessageBoxHandle).Close();
                        }
                        return true;
                    },
                    () =>
                    {
                        return true;
                    });
                    return false;
                }
                else
                {
                    var close = onSelected?.Invoke(new FileDialogResponse()
                    {
                        FullPath = paths.Item2,
                        RelativePath = paths.Item1,
                        FileExists = File.Exists(paths.Item2)
                    }) ?? true;
                    return close;
                }
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
            var handle = ShowMsgBox(title, message ?? string.Empty, buttons.ToArray(), new Entity[] { fullPathLabel, filesList, filenameInput }, size: new Vector2(700, -1));
            filesList.AttachedData = handle;
            handle.Panel.AdjustHeightAutomatically = true;

            // make the save button disabled when no file is selected
            var saveBtn = handle.Buttons[0];
            saveBtn.BeforeDraw = (Entity entity) =>
            {
                // check if got any file name
                saveBtn.Enabled = !string.IsNullOrEmpty(filenameInput.Value);

                // if got filename, do advance checks
                if (saveBtn.Enabled)
                {
                    // get full path and check if override file
                    var paths = GetSelectedAndFullPath();
                    if (!options.HasFlag(FileDialogOptions.AllowOverride) && File.Exists(paths.Item2))
                    {
                        saveBtn.Enabled = false;
                    }

                    // check if a folder is selected
                    if (!options.HasFlag(FileDialogOptions.CanPickFolders) && Directory.Exists(paths.Item2))
                    {
                        saveBtn.Enabled = false;
                    }
                }
            };

            // build starting files list
            UpdateFilesList();

            // return the handle
            return handle;
        }

        /// <summary>
        /// Open a dialog to select file for loading.
        /// </summary>
        /// <param name="path">Path to start dialog in.</param>
        /// <param name="onSelected">Callback to trigger when a file was selected. Return true to close dialog, false to keep it opened.</param>
        /// <param name="onCancel">Callback to trigger when the user hit cancel.</param>
        /// <param name="options">File dialog flags.</param>
        /// <param name="filterFiles">Optional method to filter file names. Return false to hide files.</param>
        /// <param name="filterFolders">Optional method to filter folder names. Return false to hide folders.</param>
        /// <param name="title">File dialog title.</param>
        /// <param name="message">Optional message to show above files.</param>
        /// <param name="loadButtonTxt">String to show on the load file button.</param>
        /// <param name="cancelButtonTxt">String to show on the cancel button.</param>
        /// <returns>Message box handle.</returns>
        public static MessageBoxHandle OpenLoadFileDialog(string path, Func<FileDialogResponse, bool> onSelected, Action onCancel = null!, FileDialogOptions options = FileDialogOptions.Default, Func<string, bool> filterFiles = null!, Func<string, bool> filterFolders = null!, string title = "Open File..", string message = null!, string loadButtonTxt = "Open File", string cancelButtonTxt = "Cancel")
        {
            options |= FileDialogOptions.MustSelectExistingFile;
            return OpenSaveFileDialog(path, onSelected, onCancel, options, filterFiles, filterFolders, title, message, loadButtonTxt, cancelButtonTxt, null);
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
        /// Will add a 'cancel' button to close dialog without selection.
        /// </summary>
        ShowCancelButton = 1 << 3,

        /// <summary>
        /// Will add a paragraph showing the current directory full path.
        /// </summary>
        ShowDirectoryPath = 1 << 4,

        /// <summary>
        /// Will only allow picking up existing files.
        /// </summary>
        MustSelectExistingFile = 1 << 5,

        /// <summary>
        /// Can select folders and not just files.
        /// </summary>
        CanPickFolders = 1 << 6,

        /// <summary>
        /// Default file dialog options.
        /// </summary>
        Default = AllowEnterFolders | AllowOverride | ShowCancelButton | ShowDirectoryPath
    }
}
