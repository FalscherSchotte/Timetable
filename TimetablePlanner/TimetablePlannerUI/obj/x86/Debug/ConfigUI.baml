   M S B A M L   `   `   `  ����M  ITimetablePlannerUI, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null7 clr-namespace:TimetablePlannerUITimetablePlannerUI  5      !    TimetablePlannerUI.ConfigUI   5      \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5      R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5      Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5      @d2http://schemas.microsoft.com/expression/blend/2008     5      /tt clr-namespace:TimetablePlannerUI     5      -   5        ��5	      �� 5
   
     �� EndPoint$  1,0.5&�6    �� 
StartPoint$ 0,0.5&�6/   .��5      �  � Color$ 	#8B5487B0��6    � Offset$ 0Q�6-   � 5      $ 	#6CA3CFF1��6   $ 1Q�6-   5      .��5      �  � RowDefinitions 5   
   �� 5      $��48 �6   �� 5      $��* �6   �� 5      $��30 �6   .+�5   
   �� $��5q�6    �� Padding$ 	10,5,10,5q�6   $��1q�60    �� CornerRadius$
 12.5��6D   ���	#FF707070��5      �� 5      $  0.5,1&�6&   $ 0.5,0&�67   .��5      � $ 	#17316793��6#   $ 0Q�65   � 5      $ 	#9B175772��6#   $ 1Q�65   5      ._�5      � .+�5      ��  �� Orientation$ 
Horizontal=�6   $
��Left=�66   .+�5      �� $
��Save��6=   $��75��6L   $��Center=�6W   $��0,0,5,0q�6{   ��6   �� *SaveCommand+6{   �� 5      $��Save as��6?   $��75��6Q   $��Center=�6\   $��0,0,5,0q�6�   ��6   �� *SaveAsCommand+6�   �� 5       $	��New��6<   $��75��6J   $��0,0,5,0q�6U   ��6   �� *
NewCommand+6U   �� 5!      $
��Load��6=   $��75��6L   ��6   �� *LoadCommand+6L   5"      �� 5#      $ 
Horizontal=�6   $��Right=�66   .+�5$      �� $��Generate��6A   $	��150��6T   ��6   �� *GenerateCommand+6T   5%      5&      5'      � 5(   
   $��1��6   .+�5)      �� $��	5,2.5,5,5q�6   .@�5*      �� $��Blocks��6    � DataContext 6*   �� *BlockContext+.��5+      !   TimetablePlannerUI.BlocksUI  5,      �� 5-      $��Rooms��6    6)   �� *RoomContext+.��5.          TimetablePlannerUI.RoomsUI  5/      �� 50      $��	Lecturers��6    6-   �� *LecturerContext+.��51      $   TimetablePlannerUI.LecturersUI  52      �� 53      $��Groups��6    6*   �� *GroupContext+.��54      !   TimetablePlannerUI.GroupsUI  55      �� 56      $��Courses��6    6+   �� *CourseContext+.��57      "   TimetablePlannerUI.CoursesUI  58      59      5:      �� 5;   
   $��1��6   $��75��6   $0��*/TimetablePlannerUI;component/HsKaLogo.png��6)   $��15q�5<      $��Bottom=�6   $��Right=�67   �� 5=   
   $��Made by Florian Heberle��6   $��1��63   $��15q�6@   $��10(�6L   $��Right=�5>      $��Bottom=�60   � 5?   
   $��2��6\   $��Stretch=�6i   $��20��6�   $	��Top=�6�   $	��5,0q�6�   	 � Maximum	 6   �� *ProgressbarMax+
 � Value
 69   �� *ProgressbarValue+6�   5@      5A      