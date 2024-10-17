# wporgfkreview

Script, given wordpress.org profile URL, fetches all the plugins the user has contributed to and then fetches the reviews for each plugin along with the date of the review and the date of the author's registration on wordpress.org.
Saves the results in a JSON file for later analysis.

For example analysis see [Jupyter Notebook](./example-output/analyze.ipynb)
```json
[
  {
    "Url": "https://wordpress.org/plugins/tattoo-shop-manager/",
    "Reviews": []
  },
  {
    "Url": "https://wordpress.org/plugins/wp-live-debug/",
    "Reviews": []
  },
  {
    "Url": "https://wordpress.org/plugins/gdpr-data-request-form/",
    "Reviews": [
      {
        "Url": "https://wordpress.org/support/topic/essential-309/",
        "ReviewCreated": "2022-09-01T19:59:00",
        "AuthorUrl": "https://wordpress.org/support/users/momo-fr/",
        "AuthorCreated": "2013-08-09T00:00:00",
        "ReviewsWritten": 119
      },
      {
        "Url": "https://wordpress.org/support/topic/essential-280/",
        "ReviewCreated": "2021-08-03T09:17:00",
        "AuthorUrl": "https://wordpress.org/support/users/webaxones/",
        "AuthorCreated": "2014-05-08T00:00:00",
        "ReviewsWritten": 17
      },
...
```

It's my first web scraping project in C#. It's ugly and slow, but it works.