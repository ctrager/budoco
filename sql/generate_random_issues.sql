DO $$ 
DECLARE
   my_int int := 0;
   my_s varchar(30);
   number_of_issues int := 100; /* how many issues to generate */
BEGIN

/* delete existing issues */
truncate table issues;
ALTER SEQUENCE issues_i_id_seq RESTART WITH 1;

/* create issues */
loop
   my_int := my_int + 1;
   my_s := cast (my_int as varchar);
   insert into issues (i_description , i_created_by_user, 
   i_category, i_project, i_organization, i_status, i_priority,
   i_assigned_to_user
   ) 

values(my_s || ' ' ||  substr(md5(random()::text), 0, floor(random() * 30 + 1)::int), 1,
floor(random() * 3 + 1)::int,
floor(random() * 3 + 1)::int,
floor(random() * 3 + 1)::int,
floor(random() * 3 + 1)::int,
floor(random() * 3 + 1)::int,
floor(random() * 2)::int /*user*/
);
   if my_int = number_of_issues then
      exit; 
   end if;
end loop;

/* for the last issue created, create 10 posts  */
my_int := 0;
loop
   my_int := my_int + 1;
   insert into posts (p_issue, p_post_type, p_text, p_created_by_user)
   values(
      number_of_issues,
      'comment',
      substr(md5(random()::text), 0, floor(random() * 30 + 1)::int),
      1);
   if my_int > 10 then
      exit; 
   end if;
end loop;

END $$;
