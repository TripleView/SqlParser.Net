1.1.16版本，重构了IAstVisitor接口里方法的返回值，从void变更为SqlExpression，目的是为了递归遍历sqlExpression的过程中可以实时替换它的子节点，在遍历完成后即可获得一个全新的SqlExpression，主要用在sql替换等场景中。

1.1.19版本移除SqlExpression中的Parent属性，原因是父子expression容易循环引用，导致内存泄露，改为在AstVisitor的方法中添加VisitContext上下文
受此影响，如果你有自定义的AstVisitor，我这边提供了快速正则替换，源:public override SqlExpression (\S{1,})\((\S{1,}) (\S{1,})\)替换为 public override SqlExpression $1($2 $3, VisitContext context = null)
IAcceptVisitor接口的方法Accept(IAstVisitor visitor, VisitContext context = null)添加了VisitContext上下文，受此影响，如果你有自定义的sqlExpression, 我这边提供了快速正则替换，源:return (visitor.\S{1,})\(this\)替换为return $1(this, context)