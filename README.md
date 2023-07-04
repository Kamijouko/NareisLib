# NareisLib
## 这是一个基于原版和外星人模组HumanAlienRace（以下简称HAR）的自定义多层级渲染模组

### 1.针对原版的每个Def定义其图像
例如：  
    对于一个defName为Test_Hair的HairDef；  
    我们在另一个xml文件中定义一个MultiTexDef，defName随意，设置其originalDef属性为Test_Hair;  
    这表示此MultiTexDef是针对这个名为Test_Hair的HairDef创建的，现在它们被关联在了一起；  
    我们可以在MultiTexDef里设置许多图层，当游戏渲染我们关联的HairDef的时候，就会自动检测到我们在MultiTexDef里定义的图层并一起渲染。  
    （此为大致举例流程，实际使用请看wiki操作介绍） 
  
  可以作用于pawn身上的Def包括：BodyDef，HeadDef，HairDef，Apparel(衣服)  
### 2.自定义多层图像  
如字面意思，可以在MultiTexDef中定义多个相同或不同图层的图像；  

  #### 但在列举图层之前需要先简单了解一些RimWorld对于Pawn(小人)的渲染机制：  
    1.RimWorld对于Pawn的渲染主要使用来自Unity引擎的DrawMesh函数；  
    2.一个Pawn大致由两个部分组成，即头(Head)和身子(Body)；  
    3.而如同函数字面意思，每个部分对应一种Mesh(网格)，贴图需要绘制在Mesh上，Mesh来自游戏内部的网格池或者即时创建；  
    ps：原版RimWorld无法渲染非常大的贴图即是因为网格的大小限制；
    4.原版在绘制网格时，每层网格的间距非常小(小数点后两个零到三个零左右)而在间距小于gpu能处理的最高精度时，将由函  
    数的执行顺序决定渲染顺序(即先执行的在底部)；  
    5.Rimworld的的渲染轴相较于传统unity游戏的区别是，它使用y轴作为高度轴（正常情况下为z）,即x和z为图像的宽和高，  
    y确定该图像的显示高度。
  #### 本模组通过对原版的渲染函数进行补充和修改，定义了一些可以指定的图层：  
    图层高度顺序由上往下递增  
    1.BottomOverlay  
    2.BottomHair  
    3.BottomShell  
    4.Body    
    5.Apparel  
    6.Hand    
    7.Head  
    8.FaceMask  
    9.Hair  
    10.FrontShell  
    11.HeadMask  
    12.Hat  
    13.Overlay  
其中的Body、Apparel、Head、FaceMask、Hair均为原版Pawn的身体部件，它们分别代表Pawn的身体、服装/装备/头部装备、头、脸妆/胡须、头发，而其他图层则是围绕这几个部件展开的。它们的默认渲染顺序也同上方展示的一致。  
以上列出的图层并不是指最多只能设置13个层，以上层所起的作用是确定图层的y轴偏移基准。  
通过在xml里指定图像为以上的任意图层，然后修改它的各项属性，由此达到多图层渲染的目的即为本模组的功能。
  
    



  
  
  




