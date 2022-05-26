SELECT TOP 1 * FROM c -- you can see the RU cost in the query stats

-- If doing point lookup, id is the the most efficient
SELECT * FROM c where c.id='a4_Australia_2021092202:27:27.9018'

SELECT TOP 1 * FROM c WHERE c.trendingCountry='Australia' AND c.internalName='a12' AND c.artistListeningCountTimestamp='2021-09-22T02:27:27.8010928Z'

-- Avoid predicate functions since they invalidate the index
-- this can search the index
SELECT TOP 100 * FROM c WHERE c.artistName LIKE 'pearl%'

-- this needs to scan it
SELECT TOP 100 * FROM c WHERE STARTSWITH(LOWER(c.artistName),'pearl')

-- same intent as above but using the case insensitive option is more efficient
SELECT TOP 100 * FROM c
WHERE STARTSWITH(c.artistName, 'pearl',true)

-- composite indexes missing vs active
SELECT TOP 100 *
FROM c
WHERE c.artistName LIKE 'p%'
ORDER BY c.artistName, c.artistListeningCountTimestamp desc