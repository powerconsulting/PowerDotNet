using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PowerDotNet
{
    /// <summary>
    /// Summary description for Enumies
    /// </summary>
    public static class Enumies
    {

        public enum Errors
        {
            NotSet = 0,
            ReserverForItems = 100,
            InvalidItemId = 101,
            InactiveItemId = 102,
            NotPublishedItemId = 103,
            InvalidLanguage = 104,
            ReservedForTransactionalErrors = 200
        }

        public enum PageTypes
        {
            NotSet = 0,
            HomePage = 1,
            ItemPage = 2,
            CmsPage = 3
        }

        public enum ItemTypes
        {
            NotSet = 0,
            Video = 1,
            Audio = 2,
            Article = 3,
            Content = 4
        }

        public enum ReturnTypes
        {
            NotSet = 0,
            JSON = 1,
            XML = 2
        }

        public enum CacheKeys
        {
            //BC of implementation constrains I have to deviate frmo NotSet being set to 0, this is not ideal, but cannot be prevented bc of legacy code support.
            NotSet = -1,
            All = 0,
            Header = 1,
            Footer = 2,
            Titles = 3,
            Generic = 4
        }

        public enum Languages
        {
            NotSet = 0,
            English = 1033,
            Spanish = 3082,
            French = 1036
        }


        public enum NotificationTypes
        {
            NONE,
            WARNING,
            INFORMATION,
            SUCCESS,
            FAILURE,
            VALIDATION_ERROR,
            WAITING,
            WAITING_PROGRESS
        }

        public enum FileTypes
        {
            NotSet = 0,
            Image = 1,
            Pdf = 2
        }
    }
}