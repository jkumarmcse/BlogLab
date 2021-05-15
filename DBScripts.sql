Create Table ApplicationUser(
ApplicationUserId int NOT NULL identity(1,1),
UserName Varchar(20) not null,
NormalizedUsername Varchar(20) not null,
Email Varchar(30) not null,
NormalizedEmail  Varchar(30) not null,
FullName  Varchar(30)  null,
PasswordHas Nvarchar(max) Not null
Primary Key(ApplicationUserId)
)

Create Index [IX_ApplicationUser_NormalizedUserName] on [ApplicationUser] (NormalizedUsername)

Create Index [IX_ApplicationUser_NormalizedEmail] on [ApplicationUser] (NormalizedEmail)


Create Table Photo(
PhotoId Int Not Null Identity(1,1),
ApplicationUserID int not null,
PublicId Varchar(50) not null,
ImageUrl Varchar(250) Not null,
[Description] Varchar(30) Not Null,
PublishDate Datetime Not Null Default Getdate(),
UpdateDate Datetime Not Null Default Getdate(),
Primary Key(PhotoID),
Foreign key (ApplicationUserID) References ApplicationUser(ApplicationUserID)
)

select getdate()

Create Table Blog(
BlogId int Not null identity(1,1),
ApplicationUserID int not null ,
PhotoId int not null,
Title Varchar(50) not null,
Content Varchar(max) not null,
PublishDate Datetime not null Default Getdate(),
UpdateDate Datetime not null Default Getdate(),
ActiveInd Bit not null Default Convert(bit,1),
Primary key(BlogID),
Foreign key (ApplicationUserID) References ApplicationUser(ApplicationUserID),
Foreign key (PhotoId) References Photo(PhotoId)
)

Create Table BlogComment(
BlogCommentID int Not Null Identity(1,1),
ParentBlogCommnetId int null,
BlogId int Not null,
ApplicationUserID int not null,
Content Varchar(300) not null,
PublishDate Datetime Not null Default getdate(),
UpdateDate Datetime Not null Default getdate(),
ActiveInd Bit not null Default Convert(bit,1),
Primary key(BlogCommentID),
Foreign key (BlogId) References Blog(BlogId),
Foreign key (ApplicationUserID) References ApplicationUser(ApplicationUserID)
)


Create Schema [Aggregate]


Create View [Aggregate].[Blog]
AS

Select t1.BlogId,
		t1.ApplicationUserID,
		t2.UserName,
		t1.Title,
		t1.Content,
		t1.PhotoId,
		t1.PublishDate,
		t1.UpdateDate,
		t1.ActiveInd

	From 
		Dbo.Blog t1
	Inner join 
		dbo.ApplicationUser t2 on t1.ApplicationUserID = t2.ApplicationUserId

select * from [Aggregate].[Blog]


Create View [Aggregate].[BlogComment]
AS

Select t1.BlogCommentID,
		t1.ParentBlogCommnetId,
		t1.BlogId,
		t1.Content,
		t2.UserName,
		t1.ApplicationUserId,
		t1.PublishDate,
		t1.UpdateDate,
		t1.ActiveInd

	From 
		Dbo.BlogComment t1
	Inner join 
		dbo.ApplicationUser t2 on t1.ApplicationUserID = t2.ApplicationUserId




Create Type Dbo.AccountType as Table
(
[username] Varchar(20) not null,
NormalizedUsername Varchar(20) not null,
Email Varchar(30) not null,
NormalizedEmail  Varchar(30) not null,
FullName  Varchar(30)  null,
PasswordHas Nvarchar(max) Not null
)



Create Type Dbo.PhotoType as Table
(
PublicId Varchar(50) not null,
ImageUrl Varchar(250) Not null,
[Description] Varchar(30) Not Null
)


Create Type Dbo.BlogType as Table
(
BlogId Int not null,
Title Varchar(50) Not null,
Content Varchar(max) Not Null,
PhotoId Int null
)


Create Type Dbo.BlogCommentType as Table
(
BlogCommentID int Not Null ,
ParentBlogCommnetId int null,
BlogId int Not null,
Content Varchar(300) not null
)


Create Procedure [DBO].[Account_GetByUserName]
(
@NormalizedUsername Varchar(20)
)
As
Select   ApplicationUserId, UserName, NormalizedUsername, Email, NormalizedEmail, FullName, PasswordHas
	from ApplicationUser t1
	Where NormalizedUsername = @NormalizedUsername

Go

Create procedure [DBO].[Account_Insert]
(
	@Account AccountType Readonly

)
As

insert into ApplicationUser (UserName, NormalizedUsername, Email, NormalizedEmail, FullName, PasswordHas)

Select  UserName, NormalizedUsername, Email, NormalizedEmail, FullName, PasswordHas from  @Account;

Select Cast(Scope_identity() as Int);
 
Go

 
 
ALTER procedure [dbo].[Blog_delete]
(
@BlogId Int
)
as
Begin

Update Dbo.BlogComment Set ActiveInd = CONVERT(bit,0)
Where BlogId = @BlogId

update dbo.Blog Set PhotoId = Null , ActiveInd = CONVERT(bit,0)
Where BlogId = @BlogId

end 

Go
Create Procedure Dbo.Blog_get
(
@BlogID int
)
As
 Begin
Select  BlogId,
		ApplicationUserID,
		UserName,
		Title,
		Content,
		PhotoId,
		PublishDate,
		UpdateDate,
		ActiveInd

	From 
		 [Aggregate].Blog
		 Where BlogId = @BlogID and ActiveInd  = CONVERT(bit,1)
end

Go
Alter Procedure Dbo.Blog_All
(
@Offset int,
@PageSize int
)
As
 Begin
Select  BlogId,
		ApplicationUserID 
		UserName,
		Title,
		Content,
		PhotoId,
		PublishDate,
		UpdateDate,
		ActiveInd

	From 
		 [Aggregate].Blog
		 Where  ActiveInd  = CONVERT(bit,1)
		 Order by BlogId
		 Offset @Offset Rows
		 Fetch Next @PageSize Rows Only

Select Count(BlogId) From [Aggregate].Blog
		 Where  ActiveInd  = CONVERT(bit,1)
end



Create Procedure DBo.Blog_GetAllFamous
As

Select 
	t1.BlogId,
	t1.ApplicationUserID,
	t1.UserName,
	t1.PhotoId,
	t1.Title,
	t1.Content,
	t1.PublishDate,
	t1.ActiveInd

	From 
		[Aggregate].Blog t1
	Inner join 
		[dbo].[BlogComment] t2 on t1.BlogId = t2.BlogId
	Where
		t1.ActiveInd = CONVERT(bit,1) and 
		t2.ActiveInd = CONVERT(bit,1) 
	Group By 
		t1.BlogId,
		t1.ApplicationUserID,
		t1.UserName,
		t1.PhotoId,
		t1.Title,
		t1.Content,
		t1.PublishDate,
		t1.ActiveInd
	Order by 
		Count(t2.BlogCommentID) desc

exec Blog_GetAllFamous

Create procedure [DBO].[Blog_GetByUserID]
(
@ApplicationUserId int
)
As
Begin

	SELECT        BlogId, ApplicationUserID, UserName, Title, [Content], PhotoId, PublishDate, UpdateDate, ActiveInd
FROM            Aggregate.Blog  
where ApplicationUserID = @ApplicationUserId 
and ActiveInd = CONVERT(bit,1)

end



Create procedure [Dbo].Blog_Upsert
(
@Blog BlogType ReadOnly,
@ApplicationUserId int
)
As
Begin
	Merge Into Blog Target
	using (
	Select BlogId,
		@ApplicationUserId as [ApplicationUserId],
		Title,
		Content,
		PhotoID
		From
	@Blog
	) as Source
	On 
	(
	Target.BlogId = Source.BlogID and Target.ApplicationUserId = Source.ApplicationUserId
	)
	When Matched Then 
		Update Set
			Target.[Title] = Source.[Title],
			Target.[Content] = Source.[Content],
			Target.[PhotoID] = Source.[PhotoID],
			Target.[UpdateDate] = getdate()
	When Not Matched By Target Then 
		Insert (
		ApplicationUserId,[Title],[Content],[PhotoID]
		)
		Values (
		Source.ApplicationUserId,Source.[Title],Source.[Content],Source.[PhotoID]
		);

	Select Cast(SCOPE_IDENTITY() as Int);


End
------------------
Create procedure [Dbo].[BlogComment_Delete]
(
@BlogCommentID int
)
AS

Begin

Drop Table if Exists #BlogCommentstobeDeleted;

with cte_blogCommects
as
(
Select 
		t1.BlogCommentID,
		t1.ParentBlogCommnetId
	From
		[dbo].BlogComment t1
	Where 
		t1.BlogCommentID = @BlogCommentID

	union all 
Select 
		t2.BlogCommentID,
		t2.ParentBlogCommnetId
	From
		[dbo].BlogComment t2
		Inner join cte_blogCommects t3
			On t3.BlogCommentID = t2.ParentBlogCommnetId
)

Select 
	[BlogCommentID],[ParentBlogCommnetId]
	into #BlogCommentstobeDeleted
	From 
	cte_blogCommects

	update t1 
	set 
		t1.ActiveInd = CONVERT(bit,1),
		[UpdateDate] = getdate()
	from  
		[dbo].BlogComment t1 inner join #BlogCommentstobeDeleted t2 on t1.BlogCommentID = t2.BlogCommentID

end 



Create Procedure [DBO].BlogComment_Get
(
@BlogCommentID int
)
AS
Begin


	SELECT       t1.BlogCommentID, t1.ParentBlogCommnetId, t1.BlogId, t1.[Content], t1.UserName,t1. ApplicationUserId, t1.PublishDate,t1.UpdateDate
FROM            Aggregate.BlogComment t1
where t1.BlogCommentID = @BlogCommentID
and t1.ActiveInd = CONVERT(bit,1) 

End 


Create procedure [dbo].BlogComment_GetAll
(
@BlogId int
)
As

Select t1.BlogCommentID, t1.ParentBlogCommnetId, t1.BlogId, t1.[Content],t1.UserName, t1.ApplicationUserId, t1.PublishDate, t1.UpdateDate 
 from [Aggregate].BlogComment t1

where t1.BlogId = @BlogId
and  t1.ActiveInd = CONVERT(bit,1) 
Order by t1.UpdateDate


Create Procedure [DBO].BlogComment_Upsert
(
@BlogComment BlogCommentType Readonly,
@ApplicationUserId int

)
AS

Begin
	Merge Into [DBO].[BlogComment] Target
	using (

		Select [BlogCommentID],[ParentBlogCommnetId],[BlogId],[Content],@ApplicationUserId as ApplicationUserId
		from @BlogComment
	) as Source
	On ( Target.[BlogCommentID] = source.[BlogCommentID] and target.ApplicationUserId = source.ApplicationUserId)
	When Matched Then
		Update Set 
			Target.[Content] = Source.[Content],
			Target.[UpdateDate] = Getdate()
	When Not Matched by Target Then 
		Insert ([ParentBlogCommnetId],[BlogId],ApplicationUserId,[Content])
		Values
		( Source.[ParentBlogCommnetId],Source.[BlogId],Source.ApplicationUserId,Source.[Content] );

		Select Cast(Scope_identity() as int);
		

end 



Create procedure [Dbo].[Photo_Delete]
(@PhotoId int)
AS
Begin

	Delete From [Dbo].[Photo] where PhotoId = @PhotoId;

End


Create Procedure [Dbo].[Photo_Get]
(@PhotoId int)
AS
Begin

SELECT        t1.PhotoId, t1.ApplicationUserID, t1.PublicId, t1.ImageUrl, t1.[Description], t1.PublishDate, t1.UpdateDate
FROM            Photo AS t1
where t1.PhotoId = @PhotoId

End


Create procedure [dbo].[Photo_GetUsersID]
(@ApplicationUserID int)
AS

Begin

SELECT        t1.PhotoId, t1.ApplicationUserID, t1.PublicId, t1.ImageUrl, t1.[Description], t1.PublishDate, t1.UpdateDate
FROM            Photo AS t1
where t1.ApplicationUserID = @ApplicationUserID

end 



create procedure [dbo].[Photo_Insert]
(
@Photo PhotoType ReadOnly,
@ApplicationUserID int)
AS

Begin

Insert into Photo ( ApplicationUserID,PublicId,ImageUrl,[Description] )

 
Select   @ApplicationUserID,  PublicId,  ImageUrl,  [Description] 
FROM            @Photo AS t1

Select Cast(Scope_Identity() as int);
 

end 