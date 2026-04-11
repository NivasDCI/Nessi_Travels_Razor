namespace Transport.Model
{
    public class PageModels
    {
        public int SetStartPaging(int activePage, int PageSize)
        {
            int int_startpaging = 0;

            if (activePage == 1)
                int_startpaging = 1;
            else
            {
                int_startpaging = (((activePage - 1) * PageSize) + 1);
            }
            return int_startpaging;
        }
        public int SetEndPaging(int activePage, int PageSize)
        {
            int int_Endpaging = (activePage) * PageSize;
            return int_Endpaging;
        }
    }

}
