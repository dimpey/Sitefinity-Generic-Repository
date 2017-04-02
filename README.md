# Sitefinity Generic Repository 

A work in progress...

* For a Sitefinity Dynamic Module called Frogs with a singular type name of Frog
* Title Field: Short Text
* Image Field: Related Media - Single Selection
* Classification Field: Categories
* Classification Field: Tags
* Classification Field: Departments

```C#
public class Frog
{
    public string Title { get; set; }
    public DateTime LastModified { get; set; } 		// Include/Query any standard properties of DynamicContent
    public Image Image { get; set; } 				// Impey.Sitefinity.Repository.Fields.Image
	public List<Category> Categories { get; set; } 	// Impey.Sitefinity.Repository.Fields.Category
    public List<Tag> Tags { get; set; } 			// Impey.Sitefinity.Repository.Fields.Tag
    public List<Category> Departments { get; set; }
}

public interface IFrogRepository : ISitefinityRepository<Frog>
{
}

public class FrogRepository : SitefinityDynamicContentRepository<Frog>, IFrogRepository
{
}

// OR

public class FrogRepository : SitefinityRepository<Frog, DynamicContent>, IFrogRepository
{
}

var repo = new FrogRepository();

var frog = repo
    .GetAll()
    .Where(h => h.Title == "Kermit" && h.LastModified > DateTime.Today.AddDays(-7))
    .OrderBy(f => f.Title)
    .FirstOrDefault();
	
var frogs = repo
    .GetAll()
    .Where(h => h.Categories.Contains("Muppet"))
    .FirstOrDefault();
```
	
* For Sitefinity News Items
* Custom Image Field: Related Media - Single Selection (Thumbnail)

```C#
public class Toad
{
	public string Title { get; set; }
	public string Content { get; set; }
	public Image Thumbnail { get; set; }
	public List<Category> Categories { get; set; }
}

public interface IToadRepository : ISitefinityRepository<Toad>
{
}

public class ToadRepository : SitefinityNewsItemRepository<Toad>, IToadRepository
{
}

var repo = new ToadRepository();

var toads = repo.GetAll().ToList();
```
