   M S B A M L   `   `   `  ����?  ;MSFT, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null1clr-namespace:MSFT.AllWindowsMSFT.AllWindows  5   	   $    MSFT.AllWindows.SettingsWindow   5   	   W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   /localclr-namespace:MSFT.AllWindows     5   	   -   5          � Title$  SettingsWindow��5   	   $	��330��6    $	��599��6-    � 
ResizeMode$ NoResize=�6R   .��5	      � $��0q�6   $	��307��6   $	��Top=�6#   .+�5
   
   �� -   5         ��Name$ defaultFolderCheckBox��5
      $3��-Save automatically files in the selected path��62   $��	28,31,0,0q�6j   $
��Left=�6}   $	��256��6�   $��18��6�   $	��Top=�6�   6�   �� 5   
   -   5         ��Name$ directoryPathTextBox��5      $
��Wrap=�63   #��� ��6�   $��	28,84,0,0q�6�   $��25��6�   $
��Left=�6�   $	��242��6  $	��Top=�6  ��6G   �� *DirectoryPath+ �� UpdateSourceTrigger$ PropertyChanged=�6  �� 5   
   -   5         ��Name$ resetPathButton��5      $��Reset default path��6H   $��
417,84,0,0q�6e   $��25��6y   $
��Left=�6�   $	��129��6�   $	��Top=�6�   �� 5   
   -   5        $ filePickerButton��5      $��Select folder��6J   $��
306,84,0,0q�6b   $��25��6v   $
��Left=�6�   $��82��6�   $	��Top=�6�   �� 5   
   -   5        $ autoReplaceCheckBox��5      $2��,Automatically replace already existing files��60   $
��Left=�6g   $��
28,167,0,0q�6�   $	��256��6�   $��25��6�   $	��Top=�6�   �� 5   
   -   5        $ autoAcceptCheckBox��5      $$��Automatically accept transfers��6/   $
��Left=�6X   $��
28,137,0,0q�6�   $	��190��6�   $��25��6�   $	��Top=�6�   �� 5   
   -   5        $ okButton��5      $��OK��6#   $��336,246,0,0q�6G   $��30��6\   $	��Top=�6h   $
��Left=�6�   $��65��6�   �� 5   
   -	   5        $ cancelButton��5      $��Cancel��6'   $��406,246,0,0q�6S   $��30��6h   $	��Top=�6t   $
��Left=�6�   $��70��6�   �� 5   
   -
   5        $ applyButton��5      $��Apply��6&   {�.  $��481,246,0,0q�6b   $��30��6w   $	��Top=�6�   $
��Left=�6�   $��65��6�   �� 5   
   $��Selected folder:��6   $��	28,59,0,0q�6+   $��25��6>   $
��Left=�6J   $��92��6e   $	��Top=�6p   �� 5   
   $
��Left=�6   $��18��6/   $��
28,114,0,0q�6;   $	��Top=�6O   $	��518��6g   5      5      