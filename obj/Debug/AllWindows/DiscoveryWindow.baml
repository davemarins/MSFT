   M S B A M L   `   `   `  ����?  ;MSFT, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null1clr-namespace:MSFT.AllWindowsMSFT.AllWindows  5   	   %    MSFT.AllWindows.DiscoveryWindow   5   	   W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   /localclr-namespace:MSFT.AllWindows     5   	   -   5          � Title$  DiscoveryWindow��5   	   $	��300��6!   $��373.404��6.    � 
ResizeMode$ NoResize=�6W   .��5	      � .+�5
   
   � -   5         �Name$ progressBar��5
      $��25��6+   $��10,184,10,0q�67   $	��Top=�6L   �� 5   
   -   5         ��Name$ startButton��5      $��Start discovery��6&   $
��Left=�6@   $��
10,229,0,0q�6[   $	��Top=�6o   $	��102��6�   6�   �� 5   
   -   5        $ 
stopButton��5      $��Stop discovery��6%   $
��Left=�6>   $��126,229,0,0q�6Y   $	��Top=�6n   $��99��6�   {�.  6�   �� 5   
   -   5         ��Name$ clientsListView��5      $��10,49,10,103q�6,    �� View 5      �� 5      .P�5      �� $
��Name��6%    �� DisplayMemberBinding 63   �� *Name+�� 5      $��
IP address��6%    69   �� *EndPoint.Address+�� 5      $
��Port��6%    63   �� *EndPoint.Port+5      5      �� 5   
   -   5        $ 
sendButton��5      $��Send file...��6%   $��246,229,10,0q�6<   $	��Top=�6R   {�.  6|   �� 5   
   -   5         ��Name$ 	infoLabel��5      $1��+Please select an host to which send a file:��6#   $��
10,16,10,0q�6Y   $	��Top=�6m   $��25��6�   5      5      