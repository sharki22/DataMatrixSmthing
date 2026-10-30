using Shop;

namespace Storage;




public class PackedStorage<T> : Storage<T>
where T : class, IProduct
{
    public Action OnIdChange;
    ///<summary> перегрузка свойства идентификатора с использованием делегата </summary > 
    public override int Id{

      get{
        return Id;
      }
    
      set{
         if (OnIdChange.GetInvocationList().Length==0) return;

         ChangeID(value);
         }
    }  

    private PackedStorage((int size, int? ID) boop) : base(boop.size) 
    {
      if (boop.ID is null){
        Id=ProductExt.NextStorageId();
      } 
      Id= boop.ID.Value;
    }


    public void ChangeID(int NewId){
      if (Id==NewId)return;
      Id=NewId;

      OnIdChange?.Invoke();
    }


    public override T? this[int index] {

      get{
      



        return this[index];
      }








    }
  


}
